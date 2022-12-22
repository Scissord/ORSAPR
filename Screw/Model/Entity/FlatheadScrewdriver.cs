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
    /// Flathead screwdriver.
    /// </summary>
    public class FlatheadScrewdriver : ScrewdriverBase
    {
        /// <summary>
        /// Flathead builder.
        /// </summary>
        /// <param name="kompasApp">Kompas application object</param>
        public FlatheadScrewdriver(KompasApplication kompasApp)
        {
            _kompasApp = kompasApp;
        }

        /// <summary>
        /// Builds flathead screwdriver.
        /// </summary>
        /// <returns>Screwdriver entity</returns>
        public override ksEntity BuildScrewdriver()
        {
            var offsetX = 0;
            var offsetY = 0;

            var inscribedCircleRadius = _kompasApp.Parameters[0] / 1.800001;

            var parameters = new double[3]{
                offsetX, offsetY, inscribedCircleRadius};

            var entity = CreateCuto(parameters);
            if (entity == null)
            {
                return null;
                
            }
            return entity;
        }
    }
}
