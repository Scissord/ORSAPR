using Kompas6Constants3D;
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
	/// Test for class "RegularPolygonParameter"
	/// </summary>
	[TestFixture]
	class RegularPolygonParameterTest
	{
		/// <summary>
		/// Test regular Polygon creation on normal parameters
		/// </summary>
		/// <param name="errorCode">Expected error code</param>
		/// <param name="anglesCount">Count of angles</param>
		/// <param name="inscribedCircleRadius">Inscribed circe radius</param>
		[TestCase(ErrorCodes.OK, 3, 10.0, TestName = "RegularPolygonParameter, normal parameters")]
		[TestCase(ErrorCodes.OK, 12, 10.0, TestName = "RegularPolygonParameter, normal parameters")]
		[TestCase(ErrorCodes.ArgumentInvalid, 2, 10.0, TestName = "RegularPolygonParameter, angles count less than 3")]
		[TestCase(ErrorCodes.ArgumentInvalid, 13, 10.0, TestName = "RegularPolygonParameter, angles count more than 12")]
		[TestCase(ErrorCodes.ArgumentInvalid, 0, 0.0, TestName = "RegularPolygonParameter, parameters = zero")]
		[TestCase(ErrorCodes.ArgumentInvalid, -2, -1.0, TestName = "RegularPolygonParameter, parameters less than zero")]
		[TestCase(ErrorCodes.ArgumentInvalid, int.MinValue, double.MinValue, TestName = "RegularPolygonParameter, min values")]
		[TestCase(ErrorCodes.ArgumentInvalid, int.MaxValue, double.MaxValue, TestName = "RegularPolygonParameter, max values")]
		[TestCase(ErrorCodes.ArgumentInvalid, 9, double.NaN, TestName = "RegularPolygonParameter, double not a number values")]
		[TestCase(ErrorCodes.ArgumentInvalid, 9, double.NegativeInfinity, TestName = "RegularPolygonParameter, double infinity values")]
		public void TestRegPolyParameterNormal(ErrorCodes errorCode, int anglesCount, double inscribedCircleRadius)
		{
			var appTest = new KompasApplicationTest();
			var app = appTest.CreateKompasApplication();

			var sketch = new KompasSketch(app.ScrewPart, Obj3dType.o3d_planeXOZ);
			var sketchEdit = sketch.BeginEntityEdit();

			var rectangleParam = new RegularPolygonParameter(app, anglesCount, inscribedCircleRadius, new KompasPoint2D(0.0, 0.0));
			sketchEdit.ksRectangle(rectangleParam.FigureParam);
			sketch.EndEntityEdit();

			Assert.AreEqual(rectangleParam.LastErrorCode, errorCode);
		}
	}
}
