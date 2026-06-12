using System;
using System.Linq;
using System.Reflection;
using Xunit;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Notifications.Services;

namespace Tests
{
    public class ModuleBoundaryTests
    {
        [Theory]
        [InlineData("Billing", new string[] { "Clinical", "Laboratory", "Pharmacy", "Patients", "Ward", "Notifications" })]
        [InlineData("Laboratory", new string[] { "Clinical", "Pharmacy", "Billing", "Patients", "Ward", "Notifications" })]
        [InlineData("Pharmacy", new string[] { "Clinical", "Laboratory", "Billing", "Patients", "Ward", "Notifications" })]
        [InlineData("Notifications", new string[] { "Clinical", "Laboratory", "Pharmacy", "Billing", "Patients", "Ward" })]
        public void VerifyNoForbiddenCrossModuleConstructorInjections(string targetModule, string[] forbiddenModules)
        {
            // Find all service implementation types in the target module
            var assembly = typeof(InvoiceService).Assembly;
            var targetNamespace = $"CareSphere.Modules.{targetModule}.Services";
            
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == targetNamespace)
                .ToList();

            foreach (var type in serviceTypes)
            {
                // Check all constructors
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        var parameterType = parameter.ParameterType;
                        
                        // Check if the parameter type belongs to any of the forbidden modules
                        foreach (var forbiddenModule in forbiddenModules)
                        {
                            var forbiddenNamespace = $"CareSphere.Modules.{forbiddenModule}.Services";
                            
                            // Check namespace prefix to block both interface and implementation
                            if (parameterType.Namespace != null && parameterType.Namespace.StartsWith(forbiddenNamespace, StringComparison.Ordinal))
                            {
                                Assert.Fail($"Module boundary violation: Service {type.FullName} in '{targetNamespace}' injects {parameterType.FullName} from forbidden module '{forbiddenModule}' via constructor.");
                            }
                        }
                    }
                }
            }
        }
    }
}
