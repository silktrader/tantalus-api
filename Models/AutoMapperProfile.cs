using AutoMapper;
using Tantalus.Entities;

namespace Tantalus.Models; 

public class AutoMapperProfile : Profile {
    public AutoMapperProfile() {
        CreateMap<FoodRequest, Food>();
        CreateMap<Food, FoodResponse>();
    }
}