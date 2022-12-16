﻿using Kompas6Constants3D;
using NUnit.Framework;
using Screw.Error;
using Screw.Model.Entity;
using Screw.Model.FigureParam;
using Screw.Model.Point;
using Screw.UnitTests.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screw.UnitTests.FigureParam
{
    /// <summary>
    /// Class for class test "RectangleParameter"
    /// </summary>
    [TestFixture]
    public class RectangleParameterTest
    {
        /// <summary>
        /// Test RectangleParameter to normal parameters
        /// </summary>
        /// <param name="errorCode">Expected error code</param>
        /// <param name="width">Width of rectangle</param>
        /// <param name="height">Height of rectangle</param>
        [TestCase(ErrorCodes.OK, 10.0, 10.0, TestName =
            "RectangleParameter, normal parameters")]
        [TestCase(ErrorCodes.ArgumentInvalid, 0.0, 0.0, TestName = 
            "RectangleParameter, parameters = zero")]
        [TestCase(ErrorCodes.ArgumentInvalid, -10.0, -10.0, TestName =
            "RectangleParameter, parameters less zero")]
        [TestCase(ErrorCodes.ArgumentInvalid, double.MaxValue, double.MinValue, 
            TestName = "RectangleParameter, double max и min value")]
        [TestCase(ErrorCodes.ArgumentInvalid, double.NaN, double.NaN, TestName = 
            "RectangleParameter, double not number value")]
        [TestCase(ErrorCodes.ArgumentInvalid, double.PositiveInfinity, 
            double.NegativeInfinity, TestName = 
            "RectangleParameter, double infinity value")]
        public void TestRectangleParameterNormal(ErrorCodes errorCode, 
            double width, double height)
        {
            var appTest = new KompasApplicationTest();
            var app = appTest.CreateKompasApplication();

            var sketch = new KompasSketch(app.ScrewPart, Obj3dType.o3d_planeXOZ);
            var sketchEdit = sketch.BeginEntityEdit();

            var rectangleParam = new RectangleParameter(app, width, height, new KompasPoint2D(0.0, 0.0));
            sketchEdit.ksRectangle(rectangleParam.FigureParam);
            sketch.EndEntityEdit();

            Assert.AreEqual(rectangleParam.LastErrorCode, errorCode);
        }
    }
}
