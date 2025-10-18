using System;
using System.Collections.Generic;
using System.Text;

namespace MediaNest.Shared.Entities; 
public interface IEntity {
    public string Id { get; set; }
    public string Title { get; set; }
}
