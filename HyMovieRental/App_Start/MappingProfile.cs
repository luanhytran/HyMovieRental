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
            Mapper.CreateMap<Customer, CustomerDto>();

            // .ForMember(...) purpose is to prevent when we create or update customer we accidentally changed the Id to another Id
            // it's mission is simply ignore the Id property when you send POST or PUT request to update the customer
            Mapper.CreateMap<CustomerDto, Customer>().ForMember(c => c.Id, opt => opt.Ignore());

            Mapper.CreateMap<Movie, MovieDto>();
            Mapper.CreateMap<MovieDto, Movie>();
        }
    }
}