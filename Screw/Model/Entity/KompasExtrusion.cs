using Kompas6API5;
using Kompas6Constants3D;
using Screw.Error;
using Screw.Validator;
using Screw.Model.FigureParam;

namespace Screw.Model.Entity
{
    /// <summary>
    /// Type of extrusion: by entity (depth and direction) 
    /// or by collecton of sketches
    /// </summary>
    public enum ExtrusionType
    {
        ByEntity,
        BySketchesCollection
    };

    /// <summary>
    /// Extrusion class.
    /// Представляет выдавливание детали 3D документа.
    /// </summary>
    public class KompasExtrusion
    {
        /// <summary>
        /// Faces count before extrusion
        /// </summary>
        private int _facesCountBeforeExtrusion;

        /// <summary>
        /// Faces count after extrusion
        /// </summary>
        private int _facesCountAfterExtrusion;

        /// <summary>
        /// Part with detail in document
        /// </summary>
        private ksPart _doc3DPart;

        /// <summary>
        /// Необходим для правильного определения базовой_основной 
        /// плоскости и параллельной базовой плоскости.
        /// </summary>
        public KompasFaces.BaseFaceAreaState BaseFaceAreaState
        {
            get;
            set;
        }

        /// <summary>
        /// Last error code
        /// </summary>
        public ErrorCodes LastErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Extruded (выдавлЕННЫЙ) entity
        /// </summary>
        private ksEntity _extrudedEntity;

        /// <summary>
        /// Extruded (выдавлЕННЫЙ) entity
        /// </summary>
        public ksEntity ExtrudedEntity
        {
            get
            {

                if (_extrudedEntity == null)
                {
                    return _extrudedEntity = GetExtrudedEntity();
                }

                else
                {
                    return _extrudedEntity;
                }
            }
        }

        /// <summary>
        /// Выдавливание по направлению, глубине и выдавливаемому объекту
        /// </summary>
        /// <param name="doc3DPart">Kompas document 3D part</param>
        /// <param name="extrusionType">Extrusion type</param>
        /// <param name="extrudableEntity">Extrudable entity</param>
        /// <param name="direction">Extrusion direction</param>
        /// <param name="depth">Extrusion depth</param>
        public KompasExtrusion(KompasExtrusionParameters parameters,
            ExtrusionType extrusionType)
        {
            if (parameters.Document3DPart == null)
            {
                LastErrorCode = ErrorCodes.KompasFigureNotSet;
                return;
            }

            var entity = (ksEntity)parameters.Document3DPart.NewEntity(
                (short)parameters.ExtrusionType);
            if (entity == null)
            {
                LastErrorCode = ErrorCodes.ExtrusionEntityCreationError;
                return;
            }

            if (parameters.ExtrusionType != Obj3dType.o3d_baseLoft)
            {
                if (parameters.ExtrudableEntity == null)
                {
                    LastErrorCode = ErrorCodes.ExtrudableEntityNotSet;
                    return;
                }
            }

            bool normalDirection = true;

            if (extrusionType == ExtrusionType.ByEntity)
            {
                if (parameters.Direction == Direction_Type.dtNormal)
                {
                    normalDirection = true;
                }
                else if (parameters.Direction == Direction_Type.dtReverse)
                {
                    normalDirection = false;
                }
                else
                {
                    LastErrorCode = ErrorCodes.ExtrusionDirectionNotSupported;
                    return;
                }

                if (parameters.Depth == default(double) 
                    ||!DoubleValidator.Validate(parameters.Depth))
                {
                    LastErrorCode = ErrorCodes.ArgumentInvalid;
                    return;
                }
            }

            var faceCollection = (ksEntityCollection)parameters.Document3DPart.
                EntityCollection((short)Obj3dType.o3d_face);
            _facesCountBeforeExtrusion = faceCollection.GetCount();

            switch (extrusionType)
            {
                case ExtrusionType.ByEntity:
                    if (!CreateExtrusionByDirection(
                        entity, 
                        parameters,
                        normalDirection))
                    {
                        return;
                    }
                    break;
                case ExtrusionType.BySketchesCollection:
                    if (!CreateExtrusionBySketchCollection(
                        entity, 
                        parameters))
                    {
                        return;
                    }
                    break;
            }

            _doc3DPart = parameters.Document3DPart;

            faceCollection.refresh();
            _facesCountAfterExtrusion = faceCollection.GetCount();

            if (_facesCountAfterExtrusion == _facesCountBeforeExtrusion)
            {
                LastErrorCode = ErrorCodes.ExtrusionFacesCountWrong;
                return;
            }
        }

        /// <summary>
        /// Создание экструзии на основе экструдируемого эскиза
        /// </summary>
        /// <param name="entity">Entity of extrusion</param>
        /// <param name="extrusionType">Extrusion type</param>
        /// <param name="extrudableEntity">Extrudable entity</param>
        /// <param name="direction">Extrusion direction</param>
        /// <param name="normalDirection">Normal direction of extrusion </param>
        /// <param name="depth">Extrusion depth</param>
        /// <returns>true if operation is successful; false in case of error</returns>
        private bool CreateExtrusionByDirection(ksEntity entity, 
            KompasExtrusionParameters parameters, bool normalDirection)
        {
            switch (parameters.ExtrusionType)
            {
                case Obj3dType.o3d_baseExtrusion:
                    if (!SetBaseExtrusionDefinition(entity, 
                        parameters, normalDirection))
                    {
                        return false;
                    }
                    break;
                case Obj3dType.o3d_cutExtrusion:
                    if (!SetCutExtrusionDefinition(entity, 
                        parameters, normalDirection))
                    {
                        return false;
                    }
                    break;
                default:
                    LastErrorCode = ErrorCodes.ExtrusionTypeCurrentlyNotSupported;
                    return false;
            }

            if (entity.Create() != true)
            {
                LastErrorCode = ErrorCodes.EntityCreateError;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set base extruson definition
        /// </summary>
        /// <param name="entity">Extrusion entity</param>
        /// <param name="extrudableEntity">Extrudable entity</param>
        /// <param name="direction">Extrusion direction</param>
        /// <param name="normalDirection">Normal direction of extrusion</param>
        /// <param name="depth">Extrusion depth</param>
        /// <returns>true if operation successful, false in case of error</returns>
        private bool SetBaseExtrusionDefinition(ksEntity entity, 
            KompasExtrusionParameters parameters, bool normalDirection)
        {
            var entityDefBase = (ksBaseExtrusionDefinition)entity.GetDefinition();
            if (entityDefBase == null)
            {
                LastErrorCode = ErrorCodes.EntityDefinitionNull;
                return false;
            }

            entityDefBase.directionType = (short)parameters.Direction;

            if (entityDefBase.SetSideParam(normalDirection, 
                (short)ksEndTypeEnum.etBlind, parameters.Depth) != true)
            {
                LastErrorCode = ErrorCodes.ExtrusionSetSideParamError;
                return false;
            }
            if (entityDefBase.SetSketch(parameters.ExtrudableEntity) != true)
            {
                LastErrorCode = ErrorCodes.ExtrusionSetSketchError;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Создание экструзии на основе экструдируемого эскиза
        /// </summary>
        /// <param name="entity">Entity of extrusion</param>
        /// <param name="extrusionType">Extrusion type</param>
        /// <param name="extrudableEntity">Extrudable entity</param>
        /// <param name="sketchesCollection">Sketches collection for extrusion</param>
        /// <returns>true if operation is successful; false in case of error</returns>
        private bool CreateExtrusionBySketchCollection(ksEntity entity,
            KompasExtrusionParameters parameters)
        {
            switch (parameters.ExtrusionType)
            {
                case Obj3dType.o3d_baseEvolution:
                    if (!SetBaseEvolutionDefinition(entity, parameters))
                    {
                        return false;
                    }
                    break;
                default:
                    LastErrorCode = ErrorCodes.ExtrusionTypeCurrentlyNotSupported;
                    return false;
            }

            if (entity.Create() != true)
            {
                LastErrorCode = ErrorCodes.EntityCreateError;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set cut extrusion
        /// </summary>
        /// <param name="entity">Extrusion entity</param>
        /// <param name="extrudableEntity">Extrudable entity</param>
        /// <param name="direction">Extrusion direction</param>
        /// <param name="normalDirection">Normal direction of extrusion</param>
        /// <param name="depth">Extrusion depth</param>
        /// <returns>true if operation successful, false in case of error</returns>
        private bool SetCutExtrusionDefinition(ksEntity entity, 
            KompasExtrusionParameters parameters, bool normalDirection)
        {
            var entityDefCut = (ksCutExtrusionDefinition)entity.GetDefinition();
            if (entityDefCut == null)
            {
                LastErrorCode = ErrorCodes.EntityDefinitionNull;
                return false;
            }

            entityDefCut.directionType = (short)parameters.Direction;

            if (entityDefCut.SetSideParam(normalDirection, 
                (short)ksEndTypeEnum.etBlind, parameters.Depth) != true)
            {
                LastErrorCode = ErrorCodes.ExtrusionSetSideParamError;
                return false;
            }
            if (entityDefCut.SetSketch(parameters.ExtrudableEntity) != true)
            {
                LastErrorCode = ErrorCodes.ExtrusionSetSketchError;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set base evolution extrusion definition
        /// </summary>
        /// <param name="entity">Extruson entity</param>
        /// <param name="sketchesCollection">Loft sketches collection</param>
        /// <returns>true if operation successful, false in case of error</returns>
        private bool SetBaseEvolutionDefinition(ksEntity entity,
            KompasExtrusionParameters parameters)
        {
            var entityDefEvolution = (ksBaseEvolutionDefinition)entity.GetDefinition();
            if (entityDefEvolution == null)
            {
                LastErrorCode = ErrorCodes.EntityDefinitionNull;
                return false;
            }
            if (parameters.SketchesCollection == null)
            {
                LastErrorCode = ErrorCodes.ExtrusionSketchesNull;
                return false;
            }
            if (parameters.SketchesCollection.GetCount() == 0)
            {
                LastErrorCode = ErrorCodes.ExtrusionSketchesNotSet;
                return false;
            }

            for (int i = 0, count = parameters.SketchesCollection.GetCount();
                i < count; i++)
            {
                if (i == 0)
                {
                    entityDefEvolution.PathPartArray().Add(
                        parameters.SketchesCollection.First());
                }
                else
                {
                    entityDefEvolution.PathPartArray().Add(
                        parameters.SketchesCollection.Next());
                }
            }

            if (entityDefEvolution.SetSketch(parameters.ExtrudableEntity)
                != true)
            {
                LastErrorCode = ErrorCodes.ExtrusionSetSketchError;
                return false;
            }

            return true;
        }

        /// <summary>
        ///Получить экструдированный объект
        ///<seealso cref="_baseFaceAreaState">
        ///информация о _основной базовой грани</seealso>
        /// </summary>
        /// <returns>Extruded entity</returns>
        private ksEntity GetExtrudedEntity()
        {
            int faceIndex1 = 0;
            int faceIndex2 = 0;
            int facesCount = _facesCountAfterExtrusion - _facesCountBeforeExtrusion;

            if (_doc3DPart == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return null;
            }

            if (facesCount == 2 || facesCount == 3)
            {
                KompasFaces.GetCylinderBasePlaneIndexes(_doc3DPart,
                    _facesCountBeforeExtrusion + 1,
                    _facesCountAfterExtrusion,
                    out faceIndex1, out faceIndex2);
            }

            else if (facesCount == 0 || facesCount == 1 || facesCount == 4)
            {
                LastErrorCode = ErrorCodes.ExtrusionFacesCountWrong;
                return null;
            }
            else
            {
                KompasFaces.GetRegPolyBasePlanesIndexes(_doc3DPart,
                _facesCountBeforeExtrusion + 1,
                _facesCountAfterExtrusion,
                out faceIndex1, out faceIndex2);
            }

            var entity = KompasFaces.GetParallelBasePlane(_doc3DPart,
                faceIndex1,
                faceIndex2,
                BaseFaceAreaState);

            return entity;
        }

        /// <summary>
        /// Установить выдавленную сущность в ноль 
        /// для дальнейшей переопределения базовой плоскости _main_
        /// </summary>
        public void ResetExtrudedEntity()
        {
            _extrudedEntity = null;
        }
    }
}
