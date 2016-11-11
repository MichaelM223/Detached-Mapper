﻿using EntityFrameworkCore.Detached.DataAnnotations;
using EntityFrameworkCore.Detached.DataAnnotations.Plugins.ManyToMany;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.Detached.Tests.Plugins.ManyToMany
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ManyToMany(nameof(Roles))]
        public IList<UserRoles> UserRoles { get; set; }

        [NotMapped]
        public IList<Role> Roles { get; set; }
    }
}
