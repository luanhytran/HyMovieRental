using AutoMapper;
using HyMovieRental.Dtos;
using HyMovieRental.Models;

namespace HyMovieRental.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // The generic take 2 param, 1 is source type and other is target type
            Mapper.CreateMap<CustomerDto, Customer>();
            Mapper.CreateMap<Customer, CustomerDto>();
        }
    }
}