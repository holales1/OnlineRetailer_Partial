using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailApi.Models
{
    public class EmailConverter : IConverter<Email, EmailDto>
    {
        public Email Convert(EmailDto sharedEmail)
        {
            return new Email
            {
                Id = sharedEmail.Id,
                Destination = sharedEmail.Destination,
                Content = sharedEmail.Content,
            };
        }

        public EmailDto Convert(Email hiddenEmail)
        {
            return new EmailDto
            {
                Id = hiddenEmail.Id,
                Destination = hiddenEmail.Destination,
                Content = hiddenEmail.Content,
            };
        }
    }
}
