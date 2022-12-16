﻿using NUnit.Framework;
using Screw.Manager;
using Screw.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screw.UnitTests
{
    /// <summary>
    /// Tests for "BuildManager" class
    /// </summary>
    [TestFixture]
    public class BuildManagerTest
    {
        /// <summary>
        /// Load the collector a certain number of times
        /// </summary>
        /// <param name="amountOfLoads">Amount of loads of builder</param>
        [TestCase(10, TestName = "Start 90 times")]
        public void LoadBuilder(int amountOfLoads)
        {
            var res = true;
            var parameters = new double[] { 27, 5, 15, 64, 10, 5 };

            for (int i = 0; i < amountOfLoads; i++)
            {
                CreateDetail(res, parameters);
            }

        }

        /// <summary>
        /// Checking for correct values
        /// </summary>
        /// <param name="expected">Expected build state:  true or false</param>
        /// <param name="parameters">Parameters</param>
        [TestCase(true, 2700, 500, 1500, 6400, 1000, 500, TestName =
            "Build manager, high values (10^^2)")]
        [TestCase(true, 270, 50, 150, 640, 100, 50, TestName =
            "Build manager, high values  (10^^1)")]
        [TestCase(true, 27, 5, 15, 64, 10, 5, TestName =
            "Build manager, correct values  (10)")]
        [TestCase(true, 2.7, 0.5, 1.5, 6.4, 1.0, 0.5, TestName =
            "Build manager, very low values  (0.1)")]
        public void CreateDetail(bool expected, params double[] parameters)
        {
            var figureParameters = new List<double>();
            for (int i = 0, length = parameters.Length; i < length; i++)
            {
                figureParameters.Add(parameters[i]);
            }

            var figureParametersValidator = 
                new FigureParametersValidator(figureParameters);
            figureParametersValidator.Validate();

            var app = new KompasApplication
            {
                Parameters = figureParameters
            };
            app.CreateDocument3D();

            var manager = new BuildManager(app);
            var result = manager.CreateDetail();

            Assert.AreEqual(result, expected);
        }

        /// <summary>
        /// Abnormal parameters
        /// </summary>
        /// <param name="expected">Expected code of error</param>
        /// <param name="parameters">Parameters</param>
        [TestCase(false, 100500, 33375, 128325, 37135, 52340, 60, TestName = 
            "Build manager, incorrect values")]
        public void CreateDetailWithUnnormalParameters(
            bool expected, params double[] parameters)
        {
            var figureParameters = new List<double>();
            for (int i = 0, length = parameters.Length; i < length; i++)
            {
                figureParameters.Add(parameters[i]);
            }

            var figureParametersValidator = 
                new FigureParametersValidator(figureParameters);
            Assert.False(figureParametersValidator.Validate(), "false expected");
        }
    }
}
