using System.Collections.Generic;

namespace Deltin.Math
{
    class EvaluateInfo
    {
        public Dictionary<string, float> InputParameters { get; }

        public EvaluateInfo(Dictionary<string, float> inputParameters)
        {
            InputParameters = inputParameters;
        }
    }
}