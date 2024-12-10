using System;
using System.IO;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.sensor_msgs;
using hakoniwa.sim;
using hakoniwa.sim.core;
using Newtonsoft.Json;
using UnityEngine;

namespace hakoniwa.sensors.lidar
{
    public class UtilTime
    {
        public static long GetUnixTime()
        {
            var baseDt = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var unixtime = (DateTimeOffset.Now - baseDt).Ticks / 10;//usec
            return unixtime;
        }

        public static bool IsTimeout(long start_time, long timeout)
        {
            long current_time = GetUnixTime();
            if (current_time >= (start_time + timeout))
            {
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class DetectionDistance
    {
        public float Min { get; set; }
        public float Max { get; set; }
    }

    [Serializable]
    public class DistanceDepedentAccuracy
    {
        public float Percentage { get; set; }
        public string NoiseDistribution { get; set; }

    }
    [Serializable]
    public class DistanceIndepedentAccuracy
    {
        public float StdDev { get; set; }
        public string NoiseDistribution { get; set; }
    }
    [Serializable]
    public class DistanceAccuracy
    {
        public string type { get; set; } //independent, dependent
        public DistanceDepedentAccuracy DistanceDepedentAccuracy { get; set; }
        public DistanceIndepedentAccuracy DistanceIndepedentAccuracy { get; set; }
    }
    [Serializable]
    public class BlindPaddingRange
    {
        public int Size { get; set; }
        public float Value { get; set; }
    }

    [Serializable]
    public class AngleRange
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public bool AscendingOrderOfData { get; set; }
        public BlindPaddingRange BlindPaddingRange { get; set; }
        public float Resolution { get; set; }
        public int ScanFrequency { get; set; }
    }

    [Serializable]
    public class InvalidMeasurement
    {
        public float Probability { get; set; }
        public string InvalidValue { get; set; }
    }

    [Serializable]
    public class LiDAR2DSensorParameters
    {
        public string frame_id { get; set; }
        public int freq { get; set; }
        public DetectionDistance DetectionDistance { get; set; }
        public DistanceAccuracy DistanceAccuracy { get; set; }
        public AngleRange AngleRange { get; set; }
        public InvalidMeasurement InvalidMeasurement { get; set; }
    }
    public class JsonParser
    {

        public static T Load<T>(string filepath)
        {
            try
            {
                string jsonString = File.ReadAllText(filepath);
                Debug.Log("filepath=" + filepath);
                var cfg = JsonConvert.DeserializeObject<T>(jsonString);
                return cfg;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
    public class LiDAR2D : MonoBehaviour, IHakoObject
    {
        public string config_filepath = "./lidar2d_spec.json";
        private GameObject sensor;
        private int update_cycle = 10;
        public LiDAR2DSensorParameters sensorParameters;
        public int max_count = 360;
        private float[] distances;
        private float angle_min = 0.0f;
        private float angle_max = 6.26573181152f;
        private float range_min = 0.119999997318f;
        private float range_max = 3.5f;
        private float angle_increment = 0.0174532923847f;
        private float time_increment = 2.98800005112e-05f;
        private float scan_time = 0.0f;
        private float[] intensities = new float[0];


        IHakoPdu hakoPdu;
        public string robotName = "LiDAR2D";
        public string pduName = "scan";

        public void EventInitialize()
        {
            this.sensor = this.gameObject;
            this.sensorParameters = JsonParser.Load<LiDAR2DSensorParameters>(config_filepath);
            this.init_angle = this.sensor.transform.localRotation;
            this.max_count = CalculateDistanceArraySize(sensorParameters.AngleRange);
            this.distances = new float[max_count];
            this.update_cycle = CalculateUpdateCycle(Time.fixedDeltaTime, sensorParameters.AngleRange.ScanFrequency);
            CalculateLaserScanParameters();

            hakoPdu = HakoAsset.GetHakoPdu();
            var ret = hakoPdu.DeclarePduForWrite(robotName, pduName);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for write: {robotName} {pduName}");
            }
        }
        int count = 0;
        public async void EventTick()
        {
            this.count++;
            if (this.count < this.update_cycle)
            {
                return;
            }
            this.count = 0;

            var pduManager = hakoPdu.GetPduManager();

            INamedPdu npdu = pduManager.CreateNamedPdu(robotName, pduName);

            this.Scan();
            this.SetScanData(npdu.Pdu);
            pduManager.WriteNamedPdu(npdu);
            var ret = await pduManager.FlushNamedPdu(npdu);
            //Debug.Log("Flush result: " + ret); 
        }
        private void Scan()
        {
            this.sensor.transform.localRotation = this.init_angle;
            int i = 0;
            float delta_yaw = this.sensorParameters.AngleRange.Resolution;
            float start_yaw = this.sensorParameters.AngleRange.Min;
            if (sensorParameters.AngleRange.AscendingOrderOfData == false)
            {
                delta_yaw = -this.sensorParameters.AngleRange.Resolution;
                start_yaw = this.sensorParameters.AngleRange.Max;
            }
            for (float yaw = start_yaw; i < this.max_count; yaw += delta_yaw)
            {
                float distance = GetSensorValue(yaw, 0, is_debug);
                //Debug.Log("v[" + i + "]=" + distances[i]);
                distances[i] = distance;
                i++;
            }
        }
        public void SetScanData(IPdu pdu)
        {
            LaserScan scan = new LaserScan(pdu);

            //header
            long t = UtilTime.GetUnixTime();
            scan.header.stamp.sec = (int)((long)(t / 1000000));
            scan.header.stamp.nanosec = (uint)((long)(t % 1000000)) * 1000;
            scan.header.frame_id = this.sensorParameters.frame_id;

            //body
            scan.angle_min = angle_min;
            scan.angle_max = angle_max;
            scan.range_min = range_min;
            scan.range_max = range_max;


            if (sensorParameters.AngleRange.BlindPaddingRange != null)
            {
                var values = new float[max_count];
                for (int i = 0; i < sensorParameters.AngleRange.BlindPaddingRange.Size; i++)
                {
                    values[i] = sensorParameters.AngleRange.BlindPaddingRange.Value;
                }
                for (int i = sensorParameters.AngleRange.BlindPaddingRange.Size; i < max_count; i++)
                {
                    values[i] = distances[i - sensorParameters.AngleRange.BlindPaddingRange.Size];
                }
                scan.ranges = values;
            }
            else
            {
                scan.ranges = distances;
            }
            scan.angle_increment = angle_increment;
            scan.time_increment = time_increment;
            scan.scan_time = scan_time;
            scan.intensities = intensities;
        }
        public void EventStart()
        {
            // nothing to do
        }

        public void EventStop()
        {
            // nothing to do
        }

        public void EventReset()
        {
            this.count = 0;
        }


        static int CalculateDistanceArraySize(AngleRange angleRange)
        {
            // Debug.Log("angleRange: " + angleRange);
            int size = (int)Math.Ceiling((angleRange.Max - angleRange.Min) / angleRange.Resolution);
            return size;
        }

        static int CalculateUpdateCycle(float fixedUpdatePeriod, int scanFrequency)
        {
            // Calculate the period of a single LiDAR scan
            float scanPeriod = 1.0f / scanFrequency;

            // Calculate the update cycle
            int updateCycle = Mathf.RoundToInt(scanPeriod / fixedUpdatePeriod);
            return updateCycle;
        }


        private Quaternion init_angle;
        public static bool is_debug = true;

        void CalculateLaserScanParameters()
        {
            // Convert degrees to radians
            this.angle_min = Mathf.Deg2Rad * (float)sensorParameters.AngleRange.Min;
            this.angle_max = Mathf.Deg2Rad * (float)sensorParameters.AngleRange.Max;

            // Convert mm to meters
            this.range_min = (float)sensorParameters.DetectionDistance.Min / 1000.0f;
            this.range_max = (float)sensorParameters.DetectionDistance.Max / 1000.0f;

            // Convert degrees to radians
            this.angle_increment = Mathf.Deg2Rad * (float)sensorParameters.AngleRange.Resolution;

            // Scan time in seconds
            this.scan_time = 1.0f / sensorParameters.AngleRange.ScanFrequency;

            // Time increment
            int numberOfMeasurements = Mathf.RoundToInt((angle_max - angle_min) / angle_increment) + 1;
            this.time_increment = scan_time / numberOfMeasurements;

            // Output the results
            Debug.Log($"angle_min: {angle_min}");
            Debug.Log($"angle_max: {angle_max}");
            Debug.Log($"range_min: {range_min}");
            Debug.Log($"range_max: {range_max}");
            Debug.Log($"angle_increment: {angle_increment}");
            Debug.Log($"time_increment: {time_increment}");
            Debug.Log($"scan_time: {scan_time}");
        }

        private float AddNoiseToDistanceDependent(float distance)
        {
            float accuracyPercentage = sensorParameters.DistanceAccuracy.DistanceDepedentAccuracy.Percentage / 100.0f;
            float noiseMean = 0;
            float noiseStandardDeviation = distance * accuracyPercentage;

            // ガウス分布ノイズを生成
            float noise = GenerateGaussianNoise(noiseMean, noiseStandardDeviation);
            float noisyDistance = distance + noise;

            // 最大値の上限を考慮
            return Mathf.Min(noisyDistance, this.range_max);
        }
        private float AddNoiseToDistanceInDependent(float distance)
        {
            float noiseMean = 0;
            float noiseStandardDeviation = sensorParameters.DistanceAccuracy.DistanceIndepedentAccuracy.StdDev;

            // ガウス分布ノイズを生成
            float noise = GenerateGaussianNoise(noiseMean, noiseStandardDeviation);
            float noisyDistance = distance + noise;

            // 最大値の上限を考慮
            return Mathf.Min(noisyDistance, this.range_max);
        }

        private float GenerateGaussianNoise(float mean, float standardDeviation)
        {
            System.Random random = new System.Random();
            double u1 = 1.0 - random.NextDouble(); // (0, 1] の一様分布
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // 標準正規分布
            double randNormal = mean + standardDeviation * randStdNormal; // 指定された平均と標準偏差による正規分布
            return (float)randNormal;
        }


        private float GetSensorValue(float degreeYaw, float degreePitch, bool debug)
        {
            // センサーの基本の前方向を取得
            Vector3 forward = sensor.transform.forward;

            // Quaternionを使用してヨー、ピッチ、ロールを一度に計算
            Quaternion yawRotation = Quaternion.AngleAxis(degreeYaw, sensor.transform.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(degreePitch, yawRotation * sensor.transform.right);

            // 最終的な回転を適用
            Quaternion finalRotation = yawRotation * pitchRotation;
            Vector3 finalDirection = finalRotation * forward;

            RaycastHit hit;

            if (Physics.Raycast(sensor.transform.position, finalDirection, out hit, this.range_max))
            {
                float distance = hit.distance;

                // ノイズを追加
                if (sensorParameters.DistanceAccuracy.type == "dependent")
                {
                    distance = AddNoiseToDistanceDependent(distance);
                }
                else
                {
                    distance = AddNoiseToDistanceInDependent(distance);
                }

                if (debug)
                {
                    Debug.DrawRay(sensor.transform.position, finalDirection * distance, Color.red, 0.05f, false);
                }

                return distance;
            }
            else
            {
                if (debug)
                {
                    Debug.DrawRay(sensor.transform.position, finalDirection * this.range_max, Color.green, 0.05f, false);
                }
                return this.range_max;
            }
        }


    }
}

