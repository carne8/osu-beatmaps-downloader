# Save Neekoteme beatmaps
This is a small project that should help you for re-download your **most played** [osu](https://osu.ppy.sh) beatmaps

# Usage
1. Download the program [here](https://github.com/carne8/osu-beatmaps-downloader/releases/latest)
1. To make the program work, you need to configure it.
	1. Find the user whose beatmaps you want, go on his osu webpage, and copy the number at the and of the url. Put them in the `config.yml` file you downloaded in step 1
	1. Go to your [osu settings](https://osu.ppy.sh/home/account/edit)
	1. Scroll down to find OAuth section
	1. Click on `New OAuth Application`
	1. Choose a name and click `Register application`
	1. Copy the client id and the client secret and put them in the `config.yml` file
	1. Copy the browser cookies named `osu_session` and `XSRF-TOKEN` and put them the `config.yml` file
	1. (*Optional*) You can change the output directory in the `config.yml` file
1. Run the program