﻿using System;
using System.Linq;
using UnitySpec.ErrorHandling;

namespace UnitySpec.Infrastructure
{
    public class TestPendingMessageFactory : ITestPendingMessageFactory
    {
        private readonly IErrorProvider _errorProvider;

        public TestPendingMessageFactory(IErrorProvider errorProvider)
        {
            _errorProvider = errorProvider;
        }

        public string BuildFromScenarioContext(ScenarioContext scenarioContext)
        {
            var pendingSteps = scenarioContext.PendingSteps.Distinct().OrderBy(s => s);
            return $"{_errorProvider.GetPendingStepDefinitionError().Message}{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", pendingSteps)}";
        }
    }
}
