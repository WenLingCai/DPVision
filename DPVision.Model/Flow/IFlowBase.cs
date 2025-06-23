using DPVision.Model.ROI;
using DPVision.Model.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DPVision.Model.Flow
{

    public interface IFlowBase
    {
        string FlowType { get; }
        float runTime { get; }

        void Save(string path);
        void Load(string path);
        ResultState Run(IImageDisplay control = null);
        float GetFlowRunTime();
    }
}
