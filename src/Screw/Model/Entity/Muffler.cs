using Kompas6API5;
using Kompas6Constants3D;
using Screw.Model.Point;
using Screw.Model.FigureParam;
using Screw.Model.Entity;
using Screw.Error;
using Screw.Validator;
using Screw;

namespace Screw.Model.Entity
{
    public class Muffler:ErrorObjCreation
    {
        /// <summary>
        /// Parameters of muffler
        /// </summary>
        private MufflerParameters _figureParameters;

        /// <summary>
        /// Kompas object 
        /// </summary>
        private KompasApplication _kompasApp;

        /// <summary>
        /// Muffler extrusion getter
        /// </summary>
        private KompasExtrusion Extrusion
        {
            get;
            set;
        }


        public Muffler(KompasApplication kompasApp, double basePoint,
            ksEntity basePlane = null)
        {
            _figureParameters.Document3DPart = kompasApp.ScrewPart;
            _figureParameters.Direction  = Direction_Type.dtNormal;
            _figureParameters.BasePlaneAxis = Obj3dType.o3d_planeYOZ;
            _figureParameters.BasePlanePoint = new KompasPoint2D(basePoint, basePoint);

            _kompasApp = kompasApp;

            Extrusion = CreateMuffler(_figureParameters, basePlane);
            if (Extrusion == null)
            {
                return;
            }
        }

            /// <summary>
            /// Конструктор глушителя
            /// </summary>
            /// <param name="figureParameters">Parameters of muffler</param>
            /// <param name="kompasApp">Kompas application specimen</param>
            /// <param name="basePlane">Base plane of muffler, by default is null</param>
        public Muffler(KompasApplication kompasApp, MufflerParameters figureParameters, 
            ksEntity basePlane = null)
            {
            if (kompasApp == null
                || figureParameters.Document3DPart == null
                || figureParameters.BasePlanePoint.LastErrorCode != ErrorCodes.OK
                || !(figureParameters.BasePlaneAxis == Obj3dType.o3d_planeXOY
                || figureParameters.BasePlaneAxis == Obj3dType.o3d_planeXOZ
                || figureParameters.BasePlaneAxis == Obj3dType.o3d_planeYOZ)
                || !DoubleValidator.Validate(figureParameters.BasePlanePoint.X)
                || !DoubleValidator.Validate(figureParameters.BasePlanePoint.Y))
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return;
            }
            if (!(figureParameters.Direction == Direction_Type.dtNormal
                || figureParameters.Direction == Direction_Type.dtReverse))
            {
                LastErrorCode = ErrorCodes.ArgumentInvalid;
                return;
            }

            _kompasApp = kompasApp;
            _figureParameters = figureParameters;

            Extrusion = CreateMuffler(figureParameters, basePlane);
            if (Extrusion == null)
            {
                return;
            }
        }
           
        /// <summary>
        /// Создать глушитель оси базовой плоскости
        /// </summary>
        /// <param name="figureParameters">Parameters of muffler</param>
        /// <param name="basePlane">Base plane of muffler, by default is null</param>
        /// <returns>Выдавливание глушителя или ноль, если выдавливание возвращает ошибку</returns>
        private KompasExtrusion CreateMuffler(MufflerParameters figureParameters, 
            ksEntity basePlane = null)
        {
            var muffler = new KompasSketch(figureParameters.Document3DPart, 
                figureParameters.BasePlaneAxis);

            if (basePlane != null)
            {
                muffler = new KompasSketch(figureParameters.Document3DPart, basePlane);
            }

            var mufflerSketchEdit = muffler.BeginEntityEdit();
            if (mufflerSketchEdit == null)
            {
                LastErrorCode = ErrorCodes.EntityCreateError;
                return null;
            }

            var mufflerRectangleParam = new RectangleParameter
                (
                    _kompasApp, 
                    _kompasApp.Parameters[0],
                    _kompasApp.Parameters[0],
                    figureParameters.BasePlanePoint
                );
            if (mufflerSketchEdit.ksRectangle(mufflerRectangleParam.FigureParam) == 0)
            {
                LastErrorCode = ErrorCodes.Document2DRegPolyCreateError;
                return null;
            }

            muffler.EndEntityEdit();

            var extrusionParameters = new KompasExtrusionParameters
                (
                    figureParameters.Document3DPart,
                    Obj3dType.o3d_baseExtrusion,
                    muffler.Entity,
                    figureParameters.Direction, _kompasApp.Parameters[4] / 4.0
                );

            var mufflerExtrusion = new KompasExtrusion(extrusionParameters,
                ExtrusionType.ByEntity);

            return mufflerExtrusion;
        }

        /// <summary>
        /// Удалить глушитель из 3D детали документа
        /// </summary>
        public bool DeleteDetail()
        {
            Extrusion.BaseFaceAreaState = KompasFaces.BaseFaceAreaState.BaseFaceAreaLower;
            var extruded = Extrusion.ExtrudedEntity;

            var extrusionParameters = new KompasExtrusionParameters
                (
                    _figureParameters.Document3DPart, 
                    Obj3dType.o3d_cutExtrusion,
                    extruded, _figureParameters.Direction, 
                    _kompasApp.Parameters[4] / 4.0
                );
            var mufflerDeletion = new KompasExtrusion(extrusionParameters,
                ExtrusionType.ByEntity);

            return true;
        }
    }
}
