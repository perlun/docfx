// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.TripleCrown.Hierarchy.DataContract.Hierarchy;
using Newtonsoft.Json;

namespace Microsoft.Docs.LearnValidation
{
    public class AchievementValidator : ValidatorBase
    {
        public AchievementValidator(List<LegacyManifestItem> manifestItems, string basePath, LearnValidationLogger logger)
            : base(manifestItems, basePath, logger)
        {
        }

        public override bool Validate(Dictionary<string, IValidateModel> fullItemsDict)
        {
            var validationResult = true;
            foreach (var item in Items)
            {
                var itemValid = true;
                var achievement = item as AchievementValidateModel;
                var result = achievement.ValidateMetadata();
                if (!string.IsNullOrEmpty(result))
                {
                    itemValid = false;
                    Logger.Log(LearnErrorLevel.Error, LearnErrorCode.TripleCrown_Achievement_MetadataError, file: item.SourceRelativePath, result);
                }

                item.IsValid = itemValid;
                validationResult &= itemValid;
            }

            return validationResult;
        }

        protected override void ExtractItems()
        {
            if (ManifestItems == null)
            {
                return;
            }

            Items = ManifestItems.SelectMany(m =>
            {
                var path = Path.Combine(BathPath, m.Output.TocOutput.RelativePath!);
                if (!File.Exists(path))
                {
                    path = m.Output.MetadataOutput.LinkToPath;
                }

                var achievements = JsonConvert.DeserializeObject<List<AchievementValidateModel>>(File.ReadAllText(path));

                achievements.ForEach(achievement => achievement.SourceRelativePath = m.SourceRelativePath!);

                return achievements;
            }).Cast<IValidateModel>().ToList();
        }

        /// <summary>
        /// won't be called
        /// </summary>
        protected override HierarchyItem GetHierarchyItem(ValidatorHierarchyItem validatorHierarchyItem, LegacyManifestItem manifestItem)
            => null;
    }
}
