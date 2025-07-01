using MediatR;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Features.DynamicField.Requests.Queries;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.Ad.Request.Query
{
    public class GetDynamicFieldsForAdsHandler : IRequestHandler<GetDynamicFieldsForAdsRequest, List<DynamicFieldDto>>
    {
        private readonly IDynamicFieldRepository dynamicFieldRepository;

        public GetDynamicFieldsForAdsHandler(IDynamicFieldRepository dynamicFieldRepository)
        {
            this.dynamicFieldRepository = dynamicFieldRepository;
        }

        public async Task<List<DynamicFieldDto>> Handle(GetDynamicFieldsForAdsRequest request, CancellationToken cancellationToken)
        {
            List<Rezqa.Domain.Entities.DynamicField> field = await dynamicFieldRepository.
                GetByCategoryAsync(request.CategoryId, request.SubCategoryId);


            var dynamicFields = field

                .Select(df => new DynamicFieldDto
                {
                    Id = df.Id,
                    Title = df.Title,
                    Name = df.Name,
                    Type = df.Type,
                    CategoryId = df.CategoryId,
                    CategoryTitle = df.Category.Title,
                    SubCategoryId = df.SubCategoryId,
                    shouldFilterbyParent = df.shouldFilterbyParent,
                    SubCategoryTitle = df.SubCategory != null ? df.SubCategory.Title : null,
                    Options = df.Options.Select(opt => new FieldOptionDto
                    {
                        Id = opt.Id,
                        Label = opt.Title,
                        Value = opt.Value,
                        ParentValue = opt.ParentValue,
                        DynamicFieldId = opt.DynamicFieldId
                    }).ToList()
                })
                .ToList();
            return dynamicFields;
        }
    }
}
