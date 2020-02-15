using System;
using UnityEngine;

namespace MLAgents.Sensor
{
    public class RenderTextureSensorComponent : SensorComponent
    {
        public RenderTexture renderTexture;
        public string sensorName = "RenderTextureSensor";
        public bool grayscale;

        public override ISensor CreateSensor()
        {
            return new RenderTextureSensor(this.renderTexture, this.grayscale, this.sensorName);
        }

        public override int[] GetObservationShape()
        {
            var width = this.renderTexture != null ? this.renderTexture.width : 0;
            var height = this.renderTexture != null ? this.renderTexture.height : 0;

            return new[] { height, width, this.grayscale ? 1 : 3 };
        }
    }
}
