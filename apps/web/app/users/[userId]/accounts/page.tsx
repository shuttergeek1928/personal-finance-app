"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  accountService,
  AccountTransferObject,
  AccountType,
  CreateAccountRequest,
  UpdateBalanceRequest,
  TransferMoneyRequest,
} from "../../../../services/account";
import { userService, UserTransferObject } from "../../../../services/user";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  AlertCircle,
  Plus,
  Wallet,
  ArrowRightLeft,
  ArrowDownCircle,
  ArrowUpCircle,
  Trash2,
  Star,
} from "lucide-react";

export default function UserAccountsPage() {
  const params = useParams<{ userId: string }>();

  const [currentUser, setCurrentUser] = useState<UserTransferObject | null>(
    null
  );
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal states
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isDepositOpen, setIsDepositOpen] = useState(false);
  const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);
  const [isTransferOpen, setIsTransferOpen] = useState(false);
  const [selectedAccount, setSelectedAccount] =
    useState<AccountTransferObject | null>(null);

  // Form states
  const [createData, setCreateData] = useState({
    name: "",
    type: "0",
    initialBalance: "0",
    accountNumber: "",
  });
  const [amount, setAmount] = useState("");
  const [transferData, setTransferData] = useState({
    amount: "",
    toAccountId: "",
  });
  const [actionError, setActionError] = useState<string | null>(null);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      if (!params || !params.userId) {
        setError("User ID not provided in route.");
        return;
      }

      const userRes = await userService.getUserById(params.userId);
      if (userRes.success && userRes.data) {
        const user = userRes.data;
        setCurrentUser(user);

        try {
          const accRes = await accountService.getAccountsByUserId(user.id);
          if (accRes.success && accRes.data) {
            const accountsData = Array.isArray(accRes.data)
              ? accRes.data
              : [accRes.data];
            setAccounts(accountsData);
          } else {
            setAccounts([]);
          }
        } catch (accErr: any) {
          if (accErr?.response?.status === 404) {
            setAccounts([]);
          } else {
            throw accErr;
          }
        }
      } else {
        setError(userRes.message || "Failed to find the specified user.");
      }
    } catch (err: any) {
      console.error(err);
      setError(err.message || "Failed to fetch data.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (params?.userId) {
      fetchData();
    }
  }, [params?.userId]);

  const handleCreateAccount = async () => {
    if (!currentUser) return;
    try {
      const payload: CreateAccountRequest = {
        userId: currentUser.id,
        name: createData.name,
        type: parseInt(createData.type) as AccountType,
        balance: {
          amount: parseFloat(createData.initialBalance),
          currency: "INR",
        },
        accountNumber: createData.accountNumber || null,
        description: "New User Account",
        isDefault: accounts.length === 0,
      };
      const res = await accountService.createAccount(payload);
      if (res.success && res.data) {
        setIsCreateOpen(false);
        setCreateData({
          name: "",
          type: "0",
          initialBalance: "0",
          accountNumber: "",
        });
        fetchData(); // Refresh
      } else {
        alert(res.message || "Failed to create account");
      }
    } catch (err: any) {
      alert(err.message);
    }
  };

  const handleDeposit = async () => {
    if (!selectedAccount) return;
    try {
      const payload: UpdateBalanceRequest = {
        id: selectedAccount.id,
        balance: { amount: parseFloat(amount), currency: "INR" },
        accountNumber: selectedAccount.accountNumber,
        isDeposit: true,
      };
      const res = await accountService.deposit(selectedAccount.id, payload);
      if (res.success) {
        setIsDepositOpen(false);
        setAmount("");
        fetchData();
      } else {
        alert(res.message || "Deposit failed");
      }
    } catch (err: any) {
      alert(err.message);
    }
  };

  const handleWithdraw = async () => {
    if (!selectedAccount) return;
    setActionError(null);
    try {
      const payload: UpdateBalanceRequest = {
        id: selectedAccount.id,
        balance: { amount: parseFloat(amount), currency: "INR" },
        accountNumber: selectedAccount.accountNumber,
        isDeposit: false,
      };
      const res = await accountService.withdraw(selectedAccount.id, payload);
      if (res.success) {
        setIsWithdrawOpen(false);
        setAmount("");
        fetchData();
      } else {
        setActionError(res.message || "Withdraw failed");
      }
    } catch (err: any) {
      const backendError =
        err.response?.data?.errors?.[0] || err.response?.data?.message;
      setActionError(backendError || err.message || "Withdraw failed");
    }
  };

  const handleTransfer = async () => {
    if (!selectedAccount) return;
    try {
      const payload: TransferMoneyRequest = {
        id: selectedAccount.id,
        balance: { amount: parseFloat(transferData.amount), currency: "INR" },
        accountNumber: selectedAccount.accountNumber,
        isDeposit: false,
        toAccountId: transferData.toAccountId,
      };
      const res = await accountService.transfer(payload);
      if (res.success) {
        setIsTransferOpen(false);
        setTransferData({ amount: "", toAccountId: "" });
        fetchData();
      } else {
        alert(res.message || "Transfer failed");
      }
    } catch (err: any) {
      alert(err.message);
    }
  };

  const handleSetDefault = async (accountNumber: string | null) => {
    if (!currentUser || !accountNumber) return;
    try {
      const res = await accountService.setDefault(
        currentUser.id,
        accountNumber
      );
      if (res.success) {
        fetchData();
      } else {
        alert(res.message || "Failed to set default account");
      }
    } catch (err: any) {
      alert(err.message);
    }
  };

  const handleDelete = async (accountId: string) => {
    if (!currentUser) return;
    if (confirm("Are you sure you want to delete this account?")) {
      try {
        const res = await accountService.deleteAccount(
          currentUser.id,
          accountId
        );
        if (res.success) {
          fetchData();
        } else {
          alert(res.message || "Failed to delete account");
        }
      } catch (err: any) {
        alert(err.message || "An error occurred");
      }
    }
  };

  const getAccountTypeName = (type: AccountType) => {
    switch (type) {
      case AccountType.Checking:
        return "Checking";
      case AccountType.Savings:
        return "Savings";
      case AccountType.Credit:
        return "Credit";
      case AccountType.Loan:
        return "Loan";
      default:
        return "Unknown";
    }
  };

  if (loading) {
    return (
      <div className="p-8 text-center text-muted-foreground">
        Loading accounts...
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <div className="bg-destructive/10 text-destructive p-4 rounded-md flex items-center gap-2">
          <AlertCircle className="w-5 h-5" />
          {error}
        </div>
      </div>
    );
  }

  return (
    <div className="p-8 max-w-6xl mx-auto space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Accounts Management
          </h1>
          <p className="text-muted-foreground mt-1">
            Managing accounts for{" "}
            <span className="font-semibold text-foreground">
              {currentUser?.fullName || currentUser?.email || "Unknown User"}
            </span>
          </p>
        </div>
        <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2">
              <Plus className="w-4 h-4" /> Open Account
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create New Account</DialogTitle>
              <DialogDescription>
                Fill in the details to open a new financial account.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label>Account Name</Label>
                <Input
                  value={createData.name}
                  onChange={(e) =>
                    setCreateData({ ...createData, name: e.target.value })
                  }
                  placeholder="e.g. My Savings"
                />
              </div>
              <div className="space-y-2">
                <Label>Account Type</Label>
                <Select
                  value={createData.type}
                  onValueChange={(v) =>
                    setCreateData({ ...createData, type: v })
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">Checking</SelectItem>
                    <SelectItem value="1">Savings</SelectItem>
                    <SelectItem value="2">Credit</SelectItem>
                    <SelectItem value="3">Loan</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Initial Balance (INR)</Label>
                <Input
                  type="number"
                  value={createData.initialBalance}
                  onChange={(e) =>
                    setCreateData({
                      ...createData,
                      initialBalance: e.target.value,
                    })
                  }
                />
              </div>
              <div className="space-y-2">
                <Label>Account Number</Label>
                <Input
                  value={createData.accountNumber}
                  onChange={(e) => {
                    const val = e.target.value.replace(/\D/g, "");
                    if (val.length <= 15) {
                      setCreateData({ ...createData, accountNumber: val });
                    }
                  }}
                  placeholder="e.g. 1234567890"
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsCreateOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateAccount}>Create Account</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {accounts.length === 0 ? (
        <Card className="border-dashed bg-muted/50 p-12 text-center text-muted-foreground">
          <Wallet className="w-12 h-12 mx-auto mb-4 opacity-50" />
          <p>No accounts found for this user.</p>
          <Button
            variant="outline"
            className="mt-4"
            onClick={() => setIsCreateOpen(true)}
          >
            Create One Now
          </Button>
        </Card>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {accounts.map((acc) => (
            <Card
              key={acc.id}
              className="relative overflow-hidden flex flex-col shadow-sm hover:shadow-md transition-shadow"
            >
              {acc.isDefault && (
                <div className="absolute top-0 right-0 bg-primary text-primary-foreground text-xs px-3 py-1 font-semibold rounded-bl-lg">
                  Default
                </div>
              )}
              <CardHeader>
                <CardTitle className="text-xl">
                  {acc.name || "Untitled Account"}
                </CardTitle>
                <CardDescription className="flex items-center gap-2">
                  <span className="capitalize">
                    {getAccountTypeName(acc.type)}
                  </span>
                  <span>•</span>
                  <span className="font-mono text-xs">
                    {acc.accountNumber || "Wait for assignment"}
                  </span>
                </CardDescription>
              </CardHeader>
              <CardContent className="flex-1">
                <div className="text-3xl font-bold text-primary">
                  {acc.balance?.amount?.toLocaleString("en-US", {
                    style: "currency",
                    currency: acc.balance?.currency || "INR",
                  })}
                </div>
              </CardContent>
              <CardFooter className="bg-muted/30 pt-4 flex gap-2 flex-wrap">
                <Dialog
                  open={isDepositOpen && selectedAccount?.id === acc.id}
                  onOpenChange={(val) => {
                    setIsDepositOpen(val);
                    if (val) setSelectedAccount(acc);
                  }}
                >
                  <DialogTrigger asChild>
                    <Button
                      variant="outline"
                      size="sm"
                      className="flex-1 gap-1"
                    >
                      <ArrowDownCircle className="w-4 h-4 text-green-500" />{" "}
                      Deposit
                    </Button>
                  </DialogTrigger>
                  <DialogContent>
                    <DialogHeader>
                      <DialogTitle>Deposit Funds</DialogTitle>
                      <DialogDescription>
                        Add money to {acc.name}
                      </DialogDescription>
                    </DialogHeader>
                    <div className="py-4 space-y-4">
                      <Label>Amount (INR)</Label>
                      <Input
                        type="number"
                        placeholder="0.00"
                        value={amount}
                        onChange={(e) => setAmount(e.target.value)}
                      />
                    </div>
                    <DialogFooter>
                      <Button onClick={handleDeposit}>Confirm Deposit</Button>
                    </DialogFooter>
                  </DialogContent>
                </Dialog>

                <Dialog
                  open={isWithdrawOpen && selectedAccount?.id === acc.id}
                  onOpenChange={(val) => {
                    setIsWithdrawOpen(val);
                    if (val) setSelectedAccount(acc);
                    setActionError(null);
                  }}
                >
                  <DialogTrigger asChild>
                    <Button
                      variant="outline"
                      size="sm"
                      className="flex-1 gap-1"
                    >
                      <ArrowUpCircle className="w-4 h-4 text-red-500" />{" "}
                      Withdraw
                    </Button>
                  </DialogTrigger>
                  <DialogContent>
                    <DialogHeader>
                      <DialogTitle>Withdraw Funds</DialogTitle>
                      <DialogDescription>
                        Take money from {acc.name}
                      </DialogDescription>
                    </DialogHeader>
                    <div className="py-2">
                      <Label className="mb-2 block">Amount (INR)</Label>
                      <Input
                        type="number"
                        placeholder="0.00"
                        value={amount}
                        onChange={(e) => {
                          setAmount(e.target.value);
                          setActionError(null);
                        }}
                      />
                      {actionError && (
                        <p className="text-sm text-red-500 mt-2 font-medium">
                          {actionError}
                        </p>
                      )}
                    </div>
                    <DialogFooter className="mt-4">
                      <Button variant="destructive" onClick={handleWithdraw}>
                        Confirm Withdraw
                      </Button>
                    </DialogFooter>
                  </DialogContent>
                </Dialog>

                <Dialog
                  open={isTransferOpen && selectedAccount?.id === acc.id}
                  onOpenChange={(val) => {
                    setIsTransferOpen(val);
                    if (val) setSelectedAccount(acc);
                  }}
                >
                  <DialogTrigger asChild>
                    <Button
                      variant="outline"
                      size="sm"
                      className="w-full mt-2 gap-1"
                    >
                      <ArrowRightLeft className="w-4 h-4" /> Transfer
                    </Button>
                  </DialogTrigger>
                  <DialogContent>
                    <DialogHeader>
                      <DialogTitle>Transfer Funds</DialogTitle>
                      <DialogDescription>
                        Transfer from {acc.name} to another account.
                      </DialogDescription>
                    </DialogHeader>
                    <div className="py-4 space-y-4">
                      <div className="space-y-2">
                        <Label>Destination Account</Label>
                        <Select
                          value={transferData.toAccountId}
                          onValueChange={(val) =>
                            setTransferData({
                              ...transferData,
                              toAccountId: val,
                            })
                          }
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Select destination account" />
                          </SelectTrigger>
                          <SelectContent>
                            {accounts
                              .filter((acc) => acc.id !== selectedAccount?.id)
                              .map((acc) => (
                                <SelectItem key={acc.id} value={acc.id}>
                                  {acc.name} (
                                  {acc.balance?.amount?.toLocaleString()}{" "}
                                  {acc.balance?.currency})
                                </SelectItem>
                              ))}
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Amount (INR)</Label>
                        <Input
                          type="number"
                          placeholder="0.00"
                          value={transferData.amount}
                          onChange={(e) =>
                            setTransferData({
                              ...transferData,
                              amount: e.target.value,
                            })
                          }
                        />
                      </div>
                    </div>
                    <DialogFooter>
                      <Button onClick={handleTransfer}>Confirm Transfer</Button>
                    </DialogFooter>
                  </DialogContent>
                </Dialog>

                <div className="w-full flex gap-2 mt-2">
                  {!acc.isDefault && acc.accountNumber && (
                    <Button
                      variant="outline"
                      size="sm"
                      className="flex-1 gap-1"
                      onClick={() => handleSetDefault(acc.accountNumber)}
                    >
                      <Star className="w-4 h-4 text-yellow-500" /> Set Default
                    </Button>
                  )}
                  <Button
                    variant="destructive"
                    size="sm"
                    className={
                      !acc.isDefault && acc.accountNumber
                        ? "flex-1 gap-1"
                        : "w-full gap-1"
                    }
                    onClick={() => handleDelete(acc.id)}
                  >
                    <Trash2 className="w-4 h-4" /> Delete
                  </Button>
                </div>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
