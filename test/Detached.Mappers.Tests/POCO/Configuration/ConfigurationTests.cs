﻿using System;
using Xunit;

namespace Detached.Mappers.Tests.POCO.Configuration
{
    public class ConfigurationTests
    {
        [Fact]
        public void map_renamed_property()
        {
            MapperOptions opts = new MapperOptions();
            opts.Type<User>()
                .FromType<UserDTO>()
                .Member(u => u.Id).FromMember(u => u.Key)
                .Member(u => u.Name).FromMember(u => u.UserName);

            Mapper mapper = new Mapper(opts);
            User user = mapper.Map<UserDTO, User>(new UserDTO { Key = 1, UserName = "leo" });
            Assert.Equal(1, user.Id);
            Assert.Equal("leo", user.Name);
        }

        [Fact]
        public void not_mapped_member()
        {
            MapperOptions opts = new MapperOptions();
            opts.Type<User>()
                .FromType<UserDTO>()
                .Member(u => u.Id).FromMember(u => u.Key)
                .Member(u => u.Name).Exclude();

            Mapper mapper = new Mapper(opts);
            User user = mapper.Map<UserDTO, User>(new UserDTO { Key = 1, UserName = "leo" });
            Assert.Equal(1, user.Id);
            Assert.Null(user.Name);
        }

        [Fact]
        public void map_computed_property()
        {
            DateTime dateTime = DateTime.Now;

            MapperOptions opts = new MapperOptions();
            opts.Type<User>()
                .FromType<UserDTO>()
                .Member(u => u.Id).FromMember(u => u.Key)
                .Member(u => u.Name).FromMember(u => u.UserName)
                .Member(u => u.ModifiedDate).FromValue((u, c) => dateTime);

            Mapper mapper = new Mapper(opts);
            User user = mapper.Map<UserDTO, User>(new UserDTO { Key = 1, UserName = "leo" });
            Assert.Equal(1, user.Id);
            Assert.Equal("leo", user.Name);
            Assert.Equal(dateTime, user.ModifiedDate);
        }

        class User
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public DateTime ModifiedDate { get; set; }
        }

        class UserDTO
        {
            public int Key { get; set; }

            public string UserName { get; set; }
        }
    }
}
