using System;
using System.Collections.Generic;

namespace FolderDeepSearchV2.Core.Services {
    public static class ServiceManager {
        private static readonly Dictionary<Type, object> MAP;

        public static IApplication App => Get<IApplication>();
        public static IDialogMessages Messages => Get<IDialogMessages>();
        public static IIODialogs IoDialogs => Get<IIODialogs>();
        public static IClipboard Clipboard => Get<IClipboard>();

        static ServiceManager() {
            MAP = new Dictionary<Type, object>();
        }

        public static T Get<T>() {
            if (MAP.TryGetValue(typeof(T), out object value)) {
                return (T) value;
            }
            else {
                throw new Exception("No such service for type: " + typeof(T));
            }
        }

        public static T Set<T>(T instance) {
            T old = MAP.TryGetValue(typeof(T), out object o) ? (T) o : default;
            MAP[typeof(T)] = instance;
            return old;
        }
    }
}