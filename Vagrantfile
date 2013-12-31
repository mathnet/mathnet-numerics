# see vagrantup.com

Vagrant.configure("2") do |config|

  config.vm.box = "wheezy64-mono3.2.5-fsharp3.1-dev"
  config.vm.box_url = "https://skydrive.live.com/redir.aspx?cid=84f3672f8cda3e91&resid=84F3672F8CDA3E91!29372&parid=84F3672F8CDA3E91!28978&authkey=!ABWHPlJFVtdF4bA"
  
  # default is only 384 MB RAM, 1 CPU in that box - we need some more
  config.vm.provider :virtualbox do |vb|
    vb.gui = false
    vb.customize ["modifyvm", :id, "--memory", "4096"]
    vb.customize ["modifyvm", :id, "--cpus", "4"]
  end
  
  # Shell Provisioning external:
  # config.vm.provision :shell, :path => "script.sh"
  
  # Shell Provisioning inline:
  # config.vm.provision :shell, :inline => "echo Hello, World"
  
  # Shell Provisioning inline alternative:
  # $script = <<SCRIPT
  # echo xy
  # SCRIPT
  # config.vm.provision :shell, :inline => $script

  # Create a forwarded port mapping which allows access to a specific port
  # within the machine from a port on the host machine. In the example below,
  # accessing "localhost:8080" will access port 80 on the guest machine.
  # config.vm.network :forwarded_port, guest: 80, host: 8080

  # Create a private network, which allows host-only access to the machine
  # using a specific IP.
  # config.vm.network :private_network, ip: "192.168.33.10"

  # Create a public network, which generally matched to bridged network.
  # Bridged networks make the machine appear as another physical device on
  # your network.
  # config.vm.network :public_network

  # Share an additional folder to the guest VM. The first argument is
  # the path on the host to the actual folder. The second argument is
  # the path on the guest to mount the folder. And the optional third
  # argument is a set of non-required options.
  # config.vm.synced_folder "../data", "/vagrant_data"

end
