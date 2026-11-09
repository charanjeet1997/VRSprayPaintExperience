using System.Collections.Generic;

namespace FlowSystem
{
    public class Flow
    {
        private List<FlowNode> nodes;
        private int index;

        public Flow(List<FlowNode> nodes)
        {
            this.nodes = nodes;
        }

        public void Start()
        {
            index = 0;

            if (nodes.Count > 0)
                nodes[index].Enter();
        }

        public void MoveNext()
        {
            nodes[index].Exit();
            index++;
            if (index < nodes.Count)
            {
                nodes[index].Enter();
            }
        }

        public void MoveBack()
        {
            if (index > 0)
            {
                nodes[index].Exit();
                index--;
                nodes[index].Enter();
            }
        }
    }
}
