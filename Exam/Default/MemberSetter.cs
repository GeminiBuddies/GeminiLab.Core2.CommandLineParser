using System;
using System.Reflection;

namespace Exam.Default {
    internal static class MemberSetter {
        public static void SetMember(object instance, MemberInfo member) {
            Console.WriteLine($"set {member}");
        }
        
        public static void SetMember(object instance, MemberInfo member, string value) {
            Console.WriteLine($"set {member} to {(value == null ? "<null>" : "\"" + value + "\"")}");
        }
    }
}
