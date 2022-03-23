using HutongGames.PlayMaker;
using System.Linq;
namespace UltimatumRadiance
{
    public static class FUntil
    {
        public static FsmState Addstate(this PlayMakerFSM fSM, FsmState newstate)
        {
            fSM.FsmStates.Add(newstate);
            return newstate;
        }
        public static FsmState Addstate(this PlayMakerFSM fSM, string newstatename)
        {
            FsmState newstate = new(fSM.Fsm) { Name = newstatename };
            return fSM.Addstate(newstate);
        }
        public static FsmState CopyState(this PlayMakerFSM fsm, string orig, string newname)
        {
            FsmState newstate = new(fsm.Fsm.GetState(orig)) { Name = newname };
            return newstate;
        }
        public static FsmState GetState(this PlayMakerFSM fsm, string statename)
        {
            return fsm.Fsm.GetState(statename);
        }
        public static void AddTransition(this PlayMakerFSM origfsm, string origstate, FsmEvent @event, string tostate)
        {
            origfsm.GetState(origstate).AddTransition(origstate, tostate);
        }
        public static void AddTransition(this PlayMakerFSM origfsm, string origstate, string @event, string tostate)
        {
            origfsm.GetState(origstate).AddTransition(origstate, tostate);
        }
        public static void AddTransition(this FsmState state, FsmEvent @event, string tostate)
        {
            state.Transitions= state.Transitions.Add(new FsmTransition
            {
                FsmEvent = @event,
                ToFsmState =state.Fsm.GetState(tostate),
                ToState = tostate
            }).ToArray();
        }
        public static void AddTransition(this FsmState state, string eventname, string tostate)
        {
           state.Transitions= state.Transitions.Add(new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventname)??new FsmEvent(eventname),
                ToFsmState = state.Fsm.GetState(tostate),
                ToState = tostate
            }).ToArray();
        }
        public static void RemoveTransition(this FsmState origstate, string eventname)
        {
            origstate.Transitions = origstate.Transitions.Where(trams => trams.EventName != eventname).ToArray();
        }
        public static void RemoveTransition(this PlayMakerFSM origfsm, string from, string eventname)
        {
            origfsm.GetState(from).RemoveTransition(eventname);
        }
        public static void RemoveTransitionSTS(this FsmState origstate,string to)
        {
            origstate.Transitions = origstate.Transitions.Where(trams => trams.ToState!=to ).ToArray();
        }
        public static void RemoveTransitionSTS(this PlayMakerFSM origfsm,string from, string to)
        {
            origfsm.GetState(from).RemoveTransitionSTS(to);
        }
        public static FsmTransition GetTransition(this FsmState state,string eventname)
        {
            return state.Transitions.First(tran=>tran.EventName == eventname);
        }
        public static FsmTransition GetTransition(this PlayMakerFSM origfsm,string statename, string eventname)
        {
            return origfsm.GetState(statename).GetTransition(eventname);
        }
        public static void ChangeTransition(this FsmState state,string eventname,string toState)
        {
            var tran=state.GetTransition(eventname);
            if(tran!=null)
            {
                tran.ToState = toState;
                tran.ToFsmState = state.Fsm.GetState(toState);
            }
        }
        public static void ChangeTransition(this PlayMakerFSM fsm,string statename, string eventname, string toState)
        {
            fsm.GetState(statename).ChangeTransition(eventname, toState);   
        }
        public static void InsertAction(this FsmState state, FsmStateAction action, int index)
        {
            state.Actions= state.Actions.Insert(action, index).ToArray();
            action.Init(state);
        }
        public static void InsertAction(this PlayMakerFSM fsm,string statename, FsmStateAction action, int index)
        {
            fsm.GetState(statename).InsertAction(action, index);
        }
        public static void InsertActionFirst(this PlayMakerFSM fsm, string statename, FsmStateAction action)
        {
            fsm.InsertAction(statename, action, 0);
        }
        public static void AddAction(this FsmState fsmstate, FsmStateAction action)
        {
            fsmstate.Actions= fsmstate.Actions.Add(action).ToArray();
        }
        public static void AddAction(this PlayMakerFSM fsm,string statename, FsmStateAction action)
        {
            fsm.GetState(statename).AddAction(action);
        }
        public static void RemoveAction(this FsmState origstate, int index)
        {
            origstate.Actions = origstate.Actions.Where((_, ind) => ind != index).ToArray();
        }
        public static void RemoveFirstAction<T>(this FsmState origstate)where T : FsmStateAction
        {
            origstate.Actions = origstate.Actions.RemoveFirst(ActionTarget => ActionTarget.GetType() is T).ToArray();
        }
        public static void RemoveAction(this PlayMakerFSM fsm,string statename, int index)
        {
            fsm.GetState(statename).RemoveAction(index);
        }
        public static FsmStateAction GetAction(this FsmState state,int index)
        {
            return state.Actions[index];
        }
        public static FsmStateAction GetAction(this PlayMakerFSM fsm,string statename,int index)
        {
           return fsm.GetState(statename).GetAction(index);
        }
        public static T GetAction<T>(this FsmState state,int index)where T:FsmStateAction
        {
            return state.GetAction(index) as T;
        }
       public static T GetAction<T>(this PlayMakerFSM fsm,string statename, int index) where T : FsmStateAction
        {
            return fsm.GetState(statename).GetAction<T>(index);
        }
        public static T GetFirstAction<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().First();
        }
        public static T GetFirstAction<T>(this PlayMakerFSM fsm,string statename) where T : FsmStateAction
        {
            return fsm.GetState(statename).GetFirstAction<T>();
        }
        public static void RemoveFirstAction<T>(this PlayMakerFSM fsm,string statename) where T : FsmStateAction
        {
            fsm.GetState(statename).RemoveFirstAction<T>();
        }
        




    }
}
