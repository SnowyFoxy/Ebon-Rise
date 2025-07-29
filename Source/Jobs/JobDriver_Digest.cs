using System.Collections.Generic;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_Digest : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
          Toil toil1 = Toils_General.Wait(2000);
            yield return toil1;
        }
    }
}