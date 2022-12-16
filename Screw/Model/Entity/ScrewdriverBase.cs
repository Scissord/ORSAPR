using Kompas6API5;
using Kompas6Constants3D;
using Screw.Error;
using Screw.Model.FigureParam;
using Screw.Model.Point;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screw.Model.Entity
{
    /// <summary>
	/// Base class for screwdriver builders.
	/// </summary>
	public abstract class ScrewdriverBase
    {
        /// <summary>
        /// Last error code
        /// </summary>
        public ErrorCodes LastErrorCode = ErrorCodes.OK;

        /// <summary>
        /// Kompas application object
        /// </summary>
        protected KompasApplication _kompasApp;

        /// <summary>
        /// Build screwdriver.
        /// </summary>
        /// <returns>Screwdriver entity.</returns>
        abstract public ksEntity BuildScrewdriver();

        /// <summary>
        /// Create cutoff for flathead screwdriver
        /// </summary>
        /// <returns>Created entity of cutoff</returns>
        protected ksEntity CreateCutout(double[] parameters)
        {
            var offsetX = parameters[0];
            var offsetY = parameters[1];

            var width = parameters[2];
            var height = parameters[3];

            var gost = 0.84;

            var rectangleSketch = new KompasSketch(_kompasApp.ScrewPart,
                Obj3dType.o3d_planeYOZ);
            var rectangleSketchEdit = rectangleSketch.BeginEntityEdit();
            var rectanglePoint = new KompasPoint2D(offsetX, offsetY);

            var rectangleParam = new RectangleParameter(_kompasApp, 
                width, height, rectanglePoint);
            if (rectangleSketchEdit.ksRectangle(rectangleParam.FigureParam, 0) == 0)
            {
                LastErrorCode = ErrorCodes.Document2DRegPolyCreateError;
                return null;
            }

            rectangleSketch.EndEntityEdit();

            var extrusionParameters = new KompasExtrusionParameters
                (
                    _kompasApp.ScrewPart,
                    Obj3dType.o3d_cutExtrusion,
                    rectangleSketch.Entity,
                    Direction_Type.dtNormal, 
                    _kompasApp.Parameters[1] * gost
                );

            var rectangleExtrusion = new KompasExtrusion(
                extrusionParameters, 
                ExtrusionType.ByEntity);
       
            return rectangleExtrusion.ExtrudedEntity;
        }


        /// <summary>
        /// метод создания треугольника для крестообразного шлица
        /// </summary>
        /// <param name="width">длина </param>
        /// <param name="height">высота</param>
        /// <param name="planeType">плоскость</param>
        /// <param name="type">метод выдавливания</param>
        /// <param name="extrusionHeight">высота выдавливания</param>
        /// <returns></returns>
        private ksEntity DrawTriangle(
            Double width, 
            Double height, 
            Obj3dType planeType, 
            Direction_Type type,
            Double extrusionHeight)
        {
          
            var regPolySketch = new KompasSketch(_kompasApp.ScrewPart, planeType);
            var regPolySketchEdit = regPolySketch.BeginEntityEdit();

            regPolySketchEdit.ksLineSeg(0,  width,   0,     -width, 1);
            regPolySketchEdit.ksLineSeg(0,  width,   height, 0,     1);
            regPolySketchEdit.ksLineSeg(0, -width,   height, 0,     1);
            regPolySketch.EndEntityEdit();
     
            var extrusionParameters = new KompasExtrusionParameters
                (
                    _kompasApp.ScrewPart,
                    Obj3dType.o3d_cutExtrusion,
                    regPolySketch.Entity,
                    type,
                    extrusionHeight
                );

            var regPolyExtrusion = new KompasExtrusion(extrusionParameters, 
                ExtrusionType.ByEntity);

            return regPolyExtrusion.ExtrudedEntity;
        }

      
        /// <summary>
        /// Метод построения крестообразного шлица
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected ksEntity CreateCutou(double[] parameters)
        {
            var     width  = parameters[0];
            var     height = parameters[1];
            double  extrusionHeight = ((_kompasApp.Parameters[1] * 0.84) / 2);

            var shape1 = DrawTriangle(width, height, Obj3dType.o3d_planeXOZ,
                Direction_Type.dtReverse, extrusionHeight);
            var shape2 = DrawTriangle(width, height, Obj3dType.o3d_planeXOZ, 
                Direction_Type.dtNormal,  extrusionHeight);
            var shape3 = DrawTriangle(width, height, Obj3dType.o3d_planeXOY, 
                Direction_Type.dtReverse, extrusionHeight);
            var shape4 = DrawTriangle(width, height, Obj3dType.o3d_planeXOY, 
                Direction_Type.dtNormal,  extrusionHeight);
   
            return shape1;
        }

        /// <summary>
        /// Метод построения щлица в виде многоугольника
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected ksEntity CreateCut(double[] parameters)
        {
            var offsetX = parameters[0];
            var offsetY = parameters[1];

            var width = parameters[2];

            var regPolySketch = new KompasSketch(_kompasApp.ScrewPart, Obj3dType.o3d_planeYOZ);
            var regPolySketchEdit = regPolySketch.BeginEntityEdit();
            var regPolyPoint = new KompasPoint2D(offsetX, offsetY);

            var regPolyParam = new RegularPolygonParameter(_kompasApp, 6, width , regPolyPoint);
            if (regPolySketchEdit.ksRegularPolygon(regPolyParam.FigureParam, 0) == 0)
            {
                LastErrorCode = ErrorCodes.Document2DRegPolyCreateError;
                return null;
            }

            regPolySketch.EndEntityEdit();

            var gost = 0.84;
            var extrusionParameters = new KompasExtrusionParameters
                (
                    _kompasApp.ScrewPart, 
                    Obj3dType.o3d_cutExtrusion, 
                    regPolySketch.Entity,
                    Direction_Type.dtNormal, 
                    _kompasApp.Parameters[1] * gost
                    );

            var regPolyExtrusion = new KompasExtrusion(
                extrusionParameters, ExtrusionType.ByEntity); 

            return regPolyExtrusion.ExtrudedEntity;  
        }   
    }
}
