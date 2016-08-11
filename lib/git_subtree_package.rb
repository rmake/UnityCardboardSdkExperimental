require "json"
require "fileutils"
require "optparse"
require "pathname"

class GitSubtreePackage

  PACKAGE_JSON = "git_subtree_package.json"

  def puts_flush *args
    puts *args
    STDOUT.flush
  end

  def call_system cmd
    puts_flush "cmd #{cmd}"
    unless system cmd
      raise "failed! #{cmd}"
    end
  end

  def run
    mode = nil

    opt = OptionParser.new

    options = {
      :private => false,
      :github => false,
    }

    opt.on('-p') do |v|
      @private = true
    end
    opt.on('--github') do |v|
      @github= true
    end

    argv = opt.parse(ARGV)

    case argv[0]
    when "init"
      self.init
    when "split"
      self.split argv[1..-1]
    when "push"
      self.push argv[1..-1]
    when "pull"
      self.pull argv[1..-1]
    when "add"
      self.add argv[1..-1]
    when "remove"
      self.remove argv[1..-1]
    end
  end

  def self.run
    package = GitSubtreePackage.new

    package.run

  end

  def init
    o = {
      "packages" => {},
    }

    self.write_json o
  end

  def read_json
    JSON.parse File.read(PACKAGE_JSON)
  end

  def write_json(o)
    File.write PACKAGE_JSON, JSON.pretty_generate(o)
  end

  def cd_to_root
    while !File.exist?(PACKAGE_JSON)
      c = Dir.pwd
      Dir.chdir ".."
      break if c != Dir.pwd
    end
  end

  def push_common(lib_path, repos_url, branch)
    puts_flush "lib_path #{lib_path}"
    puts_flush "repos_url #{repos_url}"

    self.call_system "git subtree push --prefix=#{lib_path} #{repos_url} #{branch}"
    self.call_system "git subtree split --prefix=#{lib_path} --rejoin"

  end

  def pull_common(lib_path, repos_url, branch)
    puts_flush "lib_path #{lib_path}"
    puts_flush "repos_url #{repos_url}"

    self.call_system "git subtree pull  -m'merge' --prefix=#{lib_path} #{repos_url} #{branch}"

  end

  def split_common(lib_path, repos_url, branch)

    o = self.read_json

    self.push_common(lib_path, repos_url, branch)

    o["packages"][lib_path] = {
      "repos_url" => repos_url
    }

    self.write_json o
  end

  def split(args)

    # ruby git_subtree_package/lib/git_subtree_package.rb split git_subtree_package ../git_subtree_package
    # ruby git_subtree_package/lib/git_subtree_package.rb split test_package tmp/test_package master
    # ruby git_subtree_package/lib/git_subtree_package.rb --github -p split test_package dycoon/test_package master
    # ruby git_subtree_package/lib/git_subtree_package.rb --github -p split git_subtree_package dycoon/git_subtree_package

    puts_flush "args #{args.inspect}"

    sub_path = args[0]
    repos_path = args[1]
    branch = args[2] || "master"

    here = Dir.pwd

    puts_flush "here #{here}"
    puts_flush "@github #{@github.inspect}"

    if @github
      self.cd_to_root
      tmp_repos = "git_subtree_package/tmp/tmp_repos"
      FileUtils.mkdir_p "git_subtree_package/tmp/tmp_repos"
      Dir.chdir "git_subtree_package/tmp/tmp_repos"
      self.call_system "git init"
      flag = @private ? "-p" : ""
      self.call_system "hub create #{flag} #{repos_path}"
      repos_url = "git@github.com:#{repos_path}.git"
      FileUtils.rm_rf "git_subtree_package/tmp"

    else
      FileUtils.mkdir_p repos_path
      Dir.chdir repos_path
      repos_absolute_path = Dir.pwd
      puts_flush "repos_absolute_path #{repos_absolute_path}"

      self.call_system "git init --bare"
    end

    Dir.chdir here
    self.cd_to_root
    root = Dir.pwd

    unless @github
      repos_url = Pathname.new(repos_absolute_path).relative_path_from(Pathname.new(root)).to_s
    end

    puts_flush "root #{root}"

    Dir.chdir here
    Dir.chdir sub_path
    sub_absolute_path = Dir.pwd

    lib_path = Pathname.new(sub_absolute_path).relative_path_from(Pathname.new(root)).to_s

    Dir.chdir root

    split_common(lib_path, repos_url, branch)

  end

  def push(args)
    # ruby git_subtree_package/lib/git_subtree_package.rb push git_subtree_package master
    # ruby git_subtree_package/lib/git_subtree_package.rb push test_package master

    here = Dir.pwd

    sub_path = args[0]
    branch = args[1] || "master"

    self.cd_to_root
    root = Dir.pwd

    lib_path = Pathname.new(File.join(here, sub_path
      )).relative_path_from(Pathname.new(root)).to_s

    o = self.read_json

    self.push_common(lib_path, o["packages"][lib_path]["repos_url"], branch)
  end

  def pull(args)
    # ruby git_subtree_package/lib/git_subtree_package.rb pull git_subtree_package master
    # ruby git_subtree_package/lib/git_subtree_package.rb pull test_package master

    here = Dir.pwd

    sub_path = args[0]
    branch = args[1] || "master"

    self.cd_to_root
    root = Dir.pwd

    lib_path = Pathname.new(File.join(here, sub_path
      )).relative_path_from(Pathname.new(root)).to_s

    o = self.read_json

    self.pull_common(lib_path, o["packages"][lib_path]["repos_url"], branch)

  end

  def add(args)
    # ruby git_subtree_package/lib/git_subtree_package.rb add git_subtree_package ../git_subtree_package
    # ruby git_subtree_package/lib/git_subtree_package.rb add test_package tmp/test_package master
    # ruby git_subtree_package/lib/git_subtree_package.rb --github add test_package dycoon/test_package master
    # ruby git_subtree_package/lib/git_subtree_package.rb --github add git_subtree_package dycoon/git_subtree_package

    puts_flush "args #{args.inspect}"

    sub_path = args[0]
    repos_path = args[1]
    branch = args[2] || "master"

    here = Dir.pwd

    puts_flush "here #{here}"
    puts_flush "@github #{@github.inspect}"

    if @github
      repos_url = "git@github.com:#{repos_path}.git"

    else
      FileUtils.mkdir_p repos_path
      Dir.chdir repos_path
      repos_absolute_path = Dir.pwd
      puts_flush "repos_absolute_path #{repos_absolute_path}"
    end

    Dir.chdir here
    self.cd_to_root
    root = Dir.pwd

    unless @github
      repos_url = Pathname.new(repos_absolute_path).relative_path_from(Pathname.new(root)).to_s
    end

    puts_flush "root #{root}"

    Dir.chdir here
    Dir.chdir sub_path
    sub_absolute_path = Dir.pwd

    lib_path = Pathname.new(sub_absolute_path).relative_path_from(Pathname.new(root)).to_s

    #
    self.call_system "git subtree add --prefix=#{lib_path} #{repos_url} #{branch}"

    #
    o = self.read_json

    o["packages"][lib_path] = {
      "repos_url" => repos_url
    }

    self.write_json o

  end

  def remove(args)
    # ruby git_subtree_package/lib/git_subtree_package.rb add git_subtree_package
    # ruby git_subtree_package/lib/git_subtree_package.rb add test_package

    sub_path = args[0]

    o = self.read_json

    o["packages"].delete lib_path

    self.write_json o

  end

end

if $0 == __FILE__ then
  GitSubtreePackage.run
end

