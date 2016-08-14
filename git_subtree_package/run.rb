require_relative "lib/git_subtree_package"


GitSubtreePackage::Manager.set_execute_path "git_subtree_package/run.rb"
GitSubtreePackage::Manager.run
