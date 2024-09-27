using System.Runtime.InteropServices;

namespace IOTWeb.Models
{
    public class AccelAndForce
    {
        public float  Timestamp { get; set; } 
        public float AccX { get; set; }
        public float AccY { get; set; }
        public float AccZ { get; set; }
        public float Force { get; set; } 
    }
}
