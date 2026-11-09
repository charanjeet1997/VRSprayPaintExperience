using UnityEngine;

namespace FlowSystem
{
    public class FlowExecuter : MonoBehaviour
    {
        private Flow flow;

        public void Run(Flow flow)
        {
            this.flow = flow;
            flow.Start();
        }
        
    }
}
