 using Kompas6API5;
using Kompas6Constants3D;
using Screw.Model.Point;
using Screw.Model.FigureParam;
using Screw.Model.Entity;
using Screw.Error;
using Screw.Validator;
using Screw.Model;
using System;

namespace Screw.Manager
{
    public class ScrewManager : ErrorObjCreation, IBuildable
    {
        /// <summary>
        /// Kompas application
        /// </summary>
        private KompasApplication _kompasApp;

        /// <summary>
        /// Type of screwdriver.
        /// </summary>
        public Screwdriver ScrewdriverType = Screwdriver.WithoutHole;

        /// <summary>
        /// Step of thread of screw
        /// </summary>
        public double ThreadStep
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor of screw manager
        /// </summary>
        public ScrewManager(KompasApplication kompasApp)
        {
            if (kompasApp == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return;
            }

            _kompasApp = kompasApp;
        }

        /// <summary>
        /// Create detail
        /// </summary>
        /// <returns>true if operation successful; false in case of error</returns>
        public bool CreateDetail()
        {
            var basePlaneOfHat = CreateHat();
            if (basePlaneOfHat == null)
            {
                return false;
            }

            if (ScrewdriverType != Screwdriver.WithoutHole)
            {
                ScrewdriverBase builder = null;

                switch (ScrewdriverType)
                {
                    case Screwdriver.CrossheadScrewdriver:
                        builder = new CrossheadScrewdriver(_kompasApp);
                        break;
                    case Screwdriver.FlatheadScrewdriver:
                        builder = new FlatheadScrewdriver(_kompasApp);
                        break;
                    case Screwdriver.RegularPolygonScrewdriver:
                        builder = new RegularPolygonScrewdriver(_kompasApp);
                        break;
                }

                builder.BuildScrewdriver();
            }

            var carvingEntities = CreateBase(basePlaneOfHat);
            if (carvingEntities == null
                || carvingEntities[0] == null
                || carvingEntities == null
            )
            {
                return false;
            }

            if (!CreateThread(carvingEntities)) return false;

            return true;
        }

        /// <summary>
        /// Создание муфлера и шляпки винта
        /// </summary>
        /// <returns>Выдавленная шляпка винта для базовой части винта</returns>
        private ksEntity CreateHat()
        {
            double basePoint   = -(_kompasApp.Parameters[0] / 5.0);
            var mufflerManager = new Muffler(_kompasApp, basePoint);

            ksEntity extruded = DrawScrewHat();

            if (!mufflerManager.DeleteDetail())
            {
                LastErrorCode = mufflerManager.LastErrorCode;
                return null;
            }

            return extruded;
        }
        /// <summary>
        /// метод создания скетча для шляпки винта
        /// </summary>
        /// <returns></returns>
        private ksEntity DrawScrewHat()
        {
            var screwHat = new KompasSketch(_kompasApp.ScrewPart, Obj3dType.o3d_planeYOZ);
        
            DrawScrew(screwHat, new KompasPoint2D(0, 0), _kompasApp.Parameters[0] / 2);

            return ExtrusionScrew(screwHat, Direction_Type.dtReverse, _kompasApp.Parameters[4] * 0.84);      
        }

        /// <summary>
        /// Create screw base with extrusion operation
        /// </summary>
        /// Width of screw base cylinder is 0.7 * D
        /// <param name="basePlaneofHat">Base plane of hat of screw</param>
        /// <returns>
        /// Carving entities: smooth part end and thread part end
        /// </returns>
        private ksEntity[] CreateBase(ksEntity basePlaneOfHat)
        {
            double coef     = (_kompasApp.Parameters[0] * 0.700) / 2.0;
            double gostbase = (_kompasApp.Parameters[0] * 0.525) / 2.0;

            var screwBase = new KompasSketch(_kompasApp.ScrewPart, basePlaneOfHat);
            DrawScrew(screwBase, new KompasPoint2D(0, 0), coef);
            var extruded1 = ExtrusionScrew(screwBase, 
                Direction_Type.dtNormal, _kompasApp.Parameters[2]);                     
            
            var screwCarving = new KompasSketch(_kompasApp.ScrewPart, extruded1);             
            DrawScrew(screwCarving, new KompasPoint2D(0, 0), gostbase);         
            var extruded2 = ExtrusionScrew(screwCarving, 
                Direction_Type.dtNormal, _kompasApp.Parameters[3]);       

            return new ksEntity[2] {extruded1, extruded2};
        }
        
        /// <summary>
        /// Метод выдавливания для гладкой части и резьбы
        /// </summary>
        /// <param name="screwBase"></param>
        /// <param name="type">тип выдавливания</param>
        /// <param name="extrusionHeight">высота выдавливания</param>
        /// <returns></returns>
        private ksEntity ExtrusionScrew(KompasSketch screwBase, 
            Direction_Type type, Double extrusionHeight)
        {
            var extrusionParameters = new KompasExtrusionParameters
                (
                    _kompasApp.ScrewPart,
                    Obj3dType.o3d_baseExtrusion,
                    screwBase.Entity,
                    type,
                    extrusionHeight
                );

            var screwCarvingExtrusion = new KompasExtrusion(extrusionParameters, 
                ExtrusionType.ByEntity);
            screwCarvingExtrusion.BaseFaceAreaState = 
                KompasFaces.BaseFaceAreaState.BaseFaceAreaLower;

            var extruded = screwCarvingExtrusion.ExtrudedEntity;

            return extruded;
        }

        /// <summary>
        /// Метод отрисовки скетчей основных частей винта
        /// гладкой и основы под резьбу
        /// </summary>
        /// <param name="screwBase"></param>
        /// <param name="screwBasePoint"> установка базовых точек для скетча</param>
        /// <param name="extrusionHeight">длинв на которую будет производиться выдавливание</param>
        /// <param name="LineStyle">стиль линии</param>
        /// <returns></returns>
        private bool DrawScrew(KompasSketch screwBase, KompasPoint2D screwBasePoint,
            Double extrusionHeight, Int32 LineStyle = 1)
        {
            var screwSketchEdit = screwBase.BeginEntityEdit();

            if (screwSketchEdit.ksCircle(screwBasePoint.X, screwBasePoint.Y, 
                extrusionHeight, LineStyle) == 0)
            {
                LastErrorCode = ErrorCodes.Document2DCircleCreatingError;
                return false;
            }
            screwBase.EndEntityEdit();
            return true;
        }

        /// <summary>
        /// Create thread of base of screw
        /// </summary>
        /// <returns>true if operation successful; false in case of error</returns>
        private bool CreateThread(ksEntity[] carvingEntities)
        {
            var coef     = 0.7;
            var gostspin = 0.037;      
            var endX    = _kompasApp.Parameters[2] + _kompasApp.Parameters[3] 
                + (_kompasApp.Parameters[4] * 0.86);
            var startY  = -(_kompasApp.Parameters[0] * coef / 2.0);  
            var endY    = -(3.0 / 4.0 * _kompasApp.Parameters[0] * coef) / 2.0; 
       
            var spinParameters = new SpinParameters
            {
                Document3DPart      = _kompasApp.ScrewPart,
                BeginSpinFace       = carvingEntities[1],
                EndSpinFace         = carvingEntities[0],
                SpinLocationPoint   = new KompasPoint2D(0, 0),
                DiameterSize        = _kompasApp.Parameters[0] * coef,      
                SpinStep            = _kompasApp.Parameters[3] * gostspin   
            };

            var screwThreadSpin = new Spin(spinParameters);
            var step = screwThreadSpin.SpinStep;

            var screwThreadSketch = new KompasSketch(_kompasApp.ScrewPart, Obj3dType.o3d_planeXOZ);
            var screwThreadEdit = screwThreadSketch.BeginEntityEdit();

            screwThreadEdit.ksLineSeg(endX - step, endY, endX, endY, 1);
            screwThreadEdit.ksLineSeg(endX, endY, endX - step / 2.0, startY, 1);
            screwThreadEdit.ksLineSeg(endX - step / 2.0, startY, endX - step, endY, 1);

            screwThreadSketch.EndEntityEdit();

            var spinCollection = (ksEntityCollection)_kompasApp.ScrewPart.EntityCollection(
                (short)Obj3dType.o3d_cylindricSpiral);
          
            spinCollection.Clear();
            spinCollection.Add(screwThreadSpin.Entity);
            spinCollection.refresh();

            if (spinCollection.GetCount() != 1)
            {
                LastErrorCode = ErrorCodes.EntityCollectionWrong;
            }

            var extrusionParameters  = new KompasExtrusionParameters(_kompasApp.ScrewPart, 
                Obj3dType.o3d_baseEvolution, screwThreadSketch.Entity, spinCollection);
            var screwThreadExtrusion = new KompasExtrusion(
                extrusionParameters, ExtrusionType.BySketchesCollection);
            
            return true;
        }
    }
}