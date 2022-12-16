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
        /// Hat diameter
        /// </summary>
        public double _diameter;

        /// <summary>
        /// Slot depth
        /// </summary>
        public double _slotDepth;

        /// <summary>
        /// Smooth part
        /// </summary>
        public double _smoothPart;

        /// <summary>
        /// Thread Part
        /// </summary>
        public double _threadPart;

        /// <summary>
        /// Hat height
        /// </summary>
        public double _hatHeight;

        /// <summary>
        /// Slot Width
        /// </summary>
        public double _slotWidth;

        /// <summary>
        /// Setting default parameters
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
        /// Default parameters
        /// </summary>
        public Default() :this ( 27, 5, 15, 64, 10, 5.4 ) { }
        
    }   

}
