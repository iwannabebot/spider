using System;
using System.Collections.Generic;
using System.Text;

namespace Spider
{
    public static class PolicyFactory
    {
        public static ISelectionPolicy DefaultSelectionPolicy()
        {
            return GetSelectionPolicy();
        }

        public static IPolitenessPolicy DefaultPolitenessPolicy()
        {
            return GetPolitenessPolicy();
        }

        public static ISelectionPolicy GetSelectionPolicy(bool crossDomain = false)
        {
            return new SelectionPolicy()
            {
                CrossDomain = crossDomain
            };
        }

        public static IPolitenessPolicy GetPolitenessPolicy(bool robotEnabled = true, bool parallelization = true)
        {
            return new PolitenessPolicy()
            {
                Parallelization = parallelization,
                RobotEnabled = robotEnabled
            };
        }
    }
}
