using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;
using AutoMapper;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        // Mapeos de autenticación
        CreateMap<User, AuthResponse>();

        // Mapeos de entidades de dominio a DTOs para respuestas
        CreateMap<Cinema, CinemaDto>();
        CreateMap<CinemaHall, CinemaHallDto>()
            .ForMember(dest => dest.CinemaName, opt => opt.MapFrom(src => src.Cinema != null ? src.Cinema.Name : null));
        CreateMap<Movie, MovieDto>();
        CreateMap<Order, OrderDto>();
        CreateMap<Ticket, TicketDto>();
        CreateMap<Seat, SeatDto>();
        CreateMap<MovieScreening, MovieScreeningDto>();
        CreateMap<Role, RoleDto>();

        CreateMap<MovieRequest, Movie>();
        CreateMap<Movie, MovieDto>();

        // Mapeos de DTOs de solicitud a entidades de dominio para crear/actualizar
        CreateMap<CinemaRequest, Cinema>();
        CreateMap<CinemaHallRequest, CinemaHall>();
        CreateMap<MovieRequest, Movie>();
        CreateMap<SeatRequest, Seat>();
        CreateMap<MovieScreeningRequest, MovieScreening>();
        CreateMap<RegisterRequest, User>();
        CreateMap<UserUpdateRequest, User>();

    }
}