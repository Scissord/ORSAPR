﻿using NUnit.Framework;
using Screw.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screw.UnitTests.Manager
{
    /// <summary>
	/// KompasApplicationTest.
	/// Class for test KompasApplication
	/// </summary>
	[TestFixture]
    public class KompasApplicationTest
    {
        /// <summary>
        /// Construct kompas application
        /// </summary>
        /// <param name="res">Expected Result</param>
        /// <param name="parameters">Parameters for build</param>
        [TestCase(ErrorCodes.OK, 27, 5, 25, 64, 10, 5, 
            TestName = "ConstructKompasApplication, normal parameters")]
        public void TestConstructKompasApplication(
            ErrorCodes res, params double[] parameters)
        {
            var figureParameters = new List<double>();
            for (int i = 0, length = parameters.Length; i < length; i++)
            {
                figureParameters.Add(parameters[i]);
            }

            var app = new KompasApplication
            {
                Parameters = figureParameters
            };
            Assert.AreEqual(app.LastErrorCode, res);

            Assert.IsTrue(app.CreateDocument3D());
        }

        /// <summary>
        /// Create Kompas application for test
        /// </summary>
        /// <returns>Kompas application</returns>
        public KompasApplication CreateKompasApplication()
        {
            var parameters = new List<double>() { 27, 5, 25, 64, 10, 5 };
            var app = new KompasApplication
            {
                Parameters = parameters
            };
            app.CreateDocument3D();

            return app;
        }

        /// <summary>
        /// Destruction Kompas application
        /// </summary>
        [TestCase(TestName = "DestructKompasApplication, normal parameters")]
        public void TestDestructKompasApplication()
        {
            var parameters = new List<double>() { 27, 5, 25, 64, 10, 5 };
            var app = new KompasApplication
            {
                Parameters = parameters
            };

            app.CreateDocument3D();

            app.DestructApp();
        }
    }
}
