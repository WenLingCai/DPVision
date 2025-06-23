using System.Collections.Generic;
using System;

namespace DPVision.Model.Camera
{
    public abstract class AbstractFactory
    {
        public AbstractFactory()
        {
        }

        public abstract AbstractCamera CreateCamera(string id);



    }
}