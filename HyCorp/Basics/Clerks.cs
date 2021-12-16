using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{


    /// <summary>
    /// Clerks:  Handle input/output
    /// </summary>
    public abstract class Clerk<Input, Output>
    {
        public Team<Input, Output> Team { get; }
        public abstract bool CheckIfTeamReady();
        public Clerk(Team<Input, Output> team)
        {
            Team = team;
        }

    }


    public abstract class NewClerk
    {

    }

    public class TypedClerk<TInput, TOutput> : NewClerk
    {

    }


    public class AnyInputClerk<Input, Output> : Clerk<Input, Output>
    {
        public AnyInputClerk(Team<Input, Output> team) : base(team)
        {
        }

        public override bool CheckIfTeamReady()
        {
            if (Team.Staffed && Team.Inputs.Count > 0) return true;
            return false;
        }
    }
}
