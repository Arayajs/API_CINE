using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using AutoMapper;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        // Nueva configuración de mapeo
        CreateMap<User, AuthResponse>();
      

        // Nueva configuración de mapeo  
        CreateMap<Cinema, CinemaDto>();

        CreateMap<Movie, MovieDto>();
     
        CreateMap<Order, OrderDto>();

        CreateMap<Ticket, TicketDto>();

      
    }
}