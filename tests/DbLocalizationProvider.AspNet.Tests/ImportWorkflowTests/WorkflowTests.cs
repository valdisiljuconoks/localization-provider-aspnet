using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbLocalizationProvider.AspNet.Import;
using DbLocalizationProvider.Import;
using Xunit;

namespace DbLocalizationProvider.AspNet.Tests.ImportWorkflowTests
{
    public class WorkflowTests
    {
        [Fact]
        public void DetectChanges_ImportingWithSingleLanguage_ExistingWithMultipleLanguages()
        {
            var existing = new List<LocalizationResource>
                           {
                               new LocalizationResource("KEY")
                               {
                                   Translations =
                                   {
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "",
                                           Value = "invariant"
                                       },
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "en",
                                           Value = "English"
                                       }
                                   }
                               }
                           };

            var importing = new List<LocalizationResource>
                                {
                                    new LocalizationResource("KEY")
                                    {
                                        Translations =
                                        {
                                            new LocalizationResourceTranslation
                                            {
                                                Language = "en-GB",
                                                Value = ""
                                            }
                                        }
                                    }
                                };

            var sut = new ResourceImportWorkflow();
            var changes = sut.DetectChanges(importing, existing);

            Assert.Empty(changes);
        }

        [Fact]
        public void DetectChanges_ImportingWithNonExistingSingleLanguage_ExistingWithMultipleDifferentLanguages()
        {
            var existing = new List<LocalizationResource>
                           {
                               new LocalizationResource("KEY")
                               {
                                   Translations =
                                   {
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "",
                                           Value = "invariant"
                                       },
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "en",
                                           Value = "English"
                                       }
                                   }
                               }
                           };

            var importing = new List<LocalizationResource>
                                {
                                    new LocalizationResource("KEY")
                                    {
                                        Translations =
                                        {
                                            new LocalizationResourceTranslation
                                            {
                                                Language = "en-GB",
                                                Value = "Great Britain"
                                            }
                                        }
                                    }
                                };

            var sut = new ResourceImportWorkflow();
            var changes = sut.DetectChanges(importing, existing);

            Assert.NotEmpty(changes);
        }

        [Fact]
        public void DetectChanges_ImportingWithSingleLanguage_ExistingWithDifferentTranslation()
        {
            var existing = new List<LocalizationResource>
                           {
                               new LocalizationResource("KEY")
                               {
                                   Translations =
                                   {
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "",
                                           Value = "invariant"
                                       },
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "en",
                                           Value = "English"
                                       }
                                   }
                               }
                           };

            var importing = new List<LocalizationResource>
                                {
                                    new LocalizationResource("KEY")
                                    {
                                        Translations =
                                        {
                                            new LocalizationResourceTranslation
                                            {
                                                Language = "en",
                                                Value = "English 2"
                                            }
                                        }
                                    }
                                };

            var sut = new ResourceImportWorkflow();
            var changes = sut.DetectChanges(importing, existing);

            Assert.NotEmpty(changes);
        }

        [Fact]
        public void DetectChanges_ImportingWithSingleLanguage_ExistingWithSameTranslation()
        {
            var existing = new List<LocalizationResource>
                           {
                               new LocalizationResource("KEY")
                               {
                                   Translations =
                                   {
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "",
                                           Value = "invariant"
                                       },
                                       new LocalizationResourceTranslation
                                       {
                                           Language = "en",
                                           Value = "English"
                                       }
                                   }
                               }
                           };

            var importing = new List<LocalizationResource>
                                {
                                    new LocalizationResource("KEY")
                                    {
                                        Translations =
                                        {
                                            new LocalizationResourceTranslation
                                            {
                                                Language = "en",
                                                Value = "English"
                                            }
                                        }
                                    }
                                };

            var sut = new ResourceImportWorkflow();
            var changes = sut.DetectChanges(importing, existing);

            Assert.Empty(changes);
        }
    }
}
