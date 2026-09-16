using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.DTOs
{
    public class PagedUsersResponseDto
    {
        public IEnumerable<UserDto> Users { get; set; } = [];
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Pages { get; set; }
    }
}
