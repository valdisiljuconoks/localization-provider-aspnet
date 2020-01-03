// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using DbLocalizationProvider.Migrations;

namespace DbLocalizationProvider
{
    public class LanguageEntities : DbContext
    {
        public LanguageEntities() : this(ConfigurationContext.Current.DbContextConnectionString) { }

        public LanguageEntities(string connectionString) : base(connectionString)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<LanguageEntities, Configuration>());
            //Configuration.LazyLoadingEnabled = false;
            //Configuration.ProxyCreationEnabled = false;

            //Database.Initialize(false);
        }

        public virtual DbSet<LocalizationResource> LocalizationResources { get; set; }

        public virtual DbSet<LocalizationResourceTranslation> LocalizationResourceTranslations { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            var resource = builder.Entity<LocalizationResource>();
            resource.HasKey(r => r.Id);
            resource.Property(r => r.ResourceKey)
                    .IsRequired()
                    .HasMaxLength(1700)
                    .HasColumnType("VARCHAR")
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                                         new IndexAnnotation(new IndexAttribute("IX_ResourceKey", 1)
                                                             {
                                                                 IsUnique = true
                                                             }));
            resource.HasMany(r => r.Translations)
                    .WithOptional()
                    .HasForeignKey(t => t.ResourceId);

            var translation = builder.Entity<LocalizationResourceTranslation>();
            translation.HasKey(t => t.Id);
            translation.Property(t => t.ResourceId).IsRequired();
            translation.Property(t => t.Language)
                       .HasColumnType("VARCHAR")
                       .HasMaxLength(10);
        }
    }
}
