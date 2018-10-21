using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using System;
using System.Linq;

namespace AutofixtureExample.TestUtil
{
    public class AutoFakeItEasyDataAttribute : AutoDataAttribute
    {
        static IFixture Customize(params Type[] customizationTypes)
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoFakeItEasyCustomization());
            customizationTypes
                ?.ToList()
                 .ForEach(ct => fixture.Customize(
                                    customization: Activator.CreateInstance(ct) as ICustomization));

            return fixture;
        }

        public AutoFakeItEasyDataAttribute(params Type[] customizationTypes)
            : base(() => Customize(customizationTypes))
        {
        }

        public AutoFakeItEasyDataAttribute()
            : base(() => Customize())
        {
        }
    }
}
