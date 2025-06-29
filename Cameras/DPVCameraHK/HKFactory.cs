using DPVision.Model.Camera;

namespace DPVision.Cameras._HK
{
    public class HKFactory : AbstractFactory
    {
        public HKFactory() : base()
        {
        }

        public override AbstractCamera CreateCamera(string id)
        {
            return new HKCamera(id);
        }
    }
}