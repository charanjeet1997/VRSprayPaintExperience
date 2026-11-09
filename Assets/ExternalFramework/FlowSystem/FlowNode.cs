using System.Collections.Generic;

namespace FlowSystem
{
    public abstract class FlowNode
    {

       
        public abstract bool ValidateFlow();

        public virtual void Enter()
        {
            
        }
        

        public virtual void Exit()
        {
        }
    }
}
