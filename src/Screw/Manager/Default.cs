using Screw.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screw.Manager
{
    public class Default
    {
 
        /// <summary>
        /// Диаметр шляпки винта
        /// </summary>
        public double _diameter;

        /// <summary>
        /// Глубина шлица
        /// </summary>
        public double _slotDepth;

        /// <summary>
        /// Длина гладкой части
        /// </summary>
        public double _smoothPart;

        /// <summary>
        /// Длина резьбы
        /// </summary>
        public double _threadPart;

        /// <summary>
        /// Высота шляпки винта
        /// </summary>
        public double _hatHeight;

        /// <summary>
        /// Ширина шлица
        /// </summary>
        public double _slotWidth;

        /// <summary>
        /// установка дефолтных параметров
        /// </summary>
        /// <param name="diam"></param>
        /// <param name="slotD"></param>
        /// <param name="smoothP"></param>
        /// <param name="threadP"></param>
        /// <param name="hatH"></param>
        /// <param name="slotW"></param>
        public Default(double diam, double slotD, double smoothP, 
            double threadP, double hatH, double slotW)
        {
            _diameter = diam;
            _slotDepth = slotD;
            _smoothPart = smoothP;
            _threadPart = threadP;
            _hatHeight = hatH;
            _slotWidth = slotW;
        }

        /// <summary>
        /// дефолтные параметры
        /// </summary>
        public Default() :this ( 27, 5, 15, 64, 10, 5 ) { }
        
    }   

}
