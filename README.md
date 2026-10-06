# Application Description

## This is a jwt authentication server with email confirmation. I added webhooks to it so that some user/app can subscribe to events of this server. To set it up:

### First we need to register in our auth server
![](./readme_img/register.png)

### Confirm email:
![](./readme_img/confirm_email.png)

### Login:
![](./readme_img/access_token.png)

### Then we need to use that access token to set up a webhook subscription in the webhook server
![](./readme_img/subscription_server.png)

![](./readme_img/create_webhook.png)

### I used the webhook.site service. Now, when, for example, we refresh our tokens:

![](./readme_img/webhook.png)

