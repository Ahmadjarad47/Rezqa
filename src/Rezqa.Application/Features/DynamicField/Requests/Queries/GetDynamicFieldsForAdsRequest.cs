using MediatR;
using Rezqa.Application.Features.DynamicField.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.DynamicField.Requests.Queries
{
    public class GetDynamicFieldsForAdsRequest : IRequest<List<DynamicFieldDto>>
    {
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
    }
}
