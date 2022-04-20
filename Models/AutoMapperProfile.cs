using AutoMapper;
using Tantalus.Entities;

namespace Tantalus.Models; 

public class AutoMapperProfile : Profile {
    public AutoMapperProfile() {
        CreateMap<FoodAddRequest, Food>();
        CreateMap<FoodUpdateRequest, Food>();
        CreateMap<Food, FoodResponse>();
        CreateMap<Portion, PortionResponse>();
    }
}