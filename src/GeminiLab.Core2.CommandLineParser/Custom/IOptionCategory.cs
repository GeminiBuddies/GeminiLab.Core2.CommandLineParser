﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IOptionCategory {
        int TryConsume(Span<string> args, object target);
    }
    
    public interface IOptionCategory<TOptionAttribute> : IOptionCategory, IAttributeCategory<TOptionAttribute> where TOptionAttribute : ParsingAttribute { }
}
