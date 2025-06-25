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
        string FlowName { get; }

        float runTime { get; }
        List<ITool> toolList { get; set; }
        List<IToolUI> toolUIList { get; set; }
        void Save(string path);
        void Load(string path);

        ResultState Run(IImageDisplay control = null);
        float GetFlowRunTime();
       
        
    }
}
