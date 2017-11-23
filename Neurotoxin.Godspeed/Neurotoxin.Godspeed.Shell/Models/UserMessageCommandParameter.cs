using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class UserMessageCommandParameter
    {
        public UserMessageViewModel ViewModel { get; set; }
        public MessageCommand Command { get; set; }
        public object Parameter { get; set; }

        public UserMessageCommandParameter(UserMessageViewModel viewModel, MessageCommand command, object parameter)
        {
            ViewModel = viewModel;
            Command = command;
            Parameter = parameter;
        }
    }
}