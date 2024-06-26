using BoDi;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using UnitySpec.ErrorHandling;
using UnitySpec.General.Configuration;

namespace UnitySpec
{
    public interface IFeatureContext : ISpecFlowContext
    {
        FeatureInfo FeatureInfo { get; }

        CultureInfo BindingCulture { get; }

        IObjectContainer FeatureContainer { get; }
    }

    public class FeatureContext : SpecFlowContext, IFeatureContext
    {
        internal FeatureContext(IObjectContainer featureContainer, FeatureInfo featureInfo, SpecFlowConfiguration specFlowConfiguration)
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            FeatureContainer = featureContainer;
            FeatureInfo = featureInfo;
            // The Generator defines the value of FeatureInfo.Language: either feature-language or language from App.config or the default
            // The runtime can define the binding-culture: Value is configured on App.config, else it is null
            BindingCulture = specFlowConfiguration.BindingCulture ?? featureInfo.Language;
        }

        #region Singleton
        private static bool isCurrentDisabled = false;
        private static FeatureContext current;

        [Obsolete("Please get the FeatureContext via Context Injection - https://go.specflow.org/Migrate-FeatureContext-Current")]
        public static FeatureContext Current
        {
            get
            {
                if (isCurrentDisabled)
                    throw new SpecFlowException("The FeatureContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the feature context to the binding class. See https://go.specflow.org/doc-multithreaded for details.");
                if (current == null)
                {
                    Debug.WriteLine("Accessing NULL FeatureContext");
                }
                return current;
            }
            internal set
            {
                if (!isCurrentDisabled)
                    current = value;
            }
        }

        internal static void DisableSingletonInstance()
        {
            isCurrentDisabled = true;
            Thread.MemoryBarrier();
            current = null;
        }
        #endregion

        public FeatureInfo FeatureInfo { get; }
        public CultureInfo BindingCulture { get; }
        public IObjectContainer FeatureContainer { get; }
        internal Stopwatch Stopwatch { get; }
    }
}