﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal sealed class PotentialLifestyleMismatchAnalyzer : IContainerAnalyzer
    {
        internal static readonly IContainerAnalyzer Instance = new PotentialLifestyleMismatchAnalyzer();

        private PotentialLifestyleMismatchAnalyzer()
        {
        }

        public DiagnosticType DiagnosticType => DiagnosticType.PotentialLifestyleMismatch;

        public string Name => "Potential Lifestyle Mismatches";

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            var mismatchCount = results.Count();
            var serviceCount = results.Select(result => result.ServiceType).Distinct().Count();

            return
                mismatchCount + " possible lifestyle " + MismatchPlural(mismatchCount) +
                " for " + serviceCount + " " + ServicePlural(serviceCount) + ".";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " possible " + MismatchPlural(count) + ".";
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers) => (
            from producer in producers
            where producer.Registration.ShouldNotBeSuppressed(this.DiagnosticType)
            from relationship in producer.GetRelationships()
            let container = producer.Registration.Container
            where LifestyleMismatchChecker.HasPossibleLifestyleMismatch(container, relationship)
            select new PotentialLifestyleMismatchDiagnosticResult(
                serviceType: producer.ServiceType,
                description: BuildRelationshipDescription(relationship),
                relationship: relationship))
            .ToArray();

        private static string BuildRelationshipDescription(KnownRelationship relationship) => 
            string.Format(CultureInfo.InvariantCulture,
                "{0} ({1}) depends on {2}{3} ({4}).",
                Helpers.ToFriendlyName(relationship.ImplementationType),
                relationship.Lifestyle.Name,
                Helpers.ToFriendlyName(relationship.Dependency.ServiceType),
                relationship.Dependency.ServiceType != relationship.Dependency.ImplementationType
                    ? " implemented by " + Helpers.ToFriendlyName(relationship.Dependency.ImplementationType)
                    : string.Empty,
                relationship.Dependency.Lifestyle.Name);

        private static string ServicePlural(int number) => number == 1 ? "service" : "services";

        private static string MismatchPlural(int number) => number == 1 ? "mismatch" : "mismatches";
    }
}